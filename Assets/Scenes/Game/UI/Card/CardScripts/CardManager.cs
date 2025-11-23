using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Reflection;
/*todo
useSelectedCards 메서드 구현
- 대표 카드를 플레이어 캐릭터 제어기로 전달하여 카드에 맞는 스킬 사용, 맞춘 적 반환
- 현재 선택된 카드 리스트와 적카드 리스트로 족보를 판정, 알맞은 효과를 적용하도록 적 클래스의 메서드 호출
*/

//카드 리소스 경로: Resources/Cards 폴더
//HandEvaluator.cs에서 족보 평가
//카드 리소스 이름 규칙: "suit_number" (예: "spade_1", "heart_13")
public class CardManager : MonoBehaviour
{
    public GameObject enemy;// (임시) 추후 삭제 예정
    [Header("UI")]
    public GameObject CardPanel;
    private Image[] bottomCards; // 카드 UI 슬롯
    public Sprite backSprite;   // 카드 뒷면
    public Image cardprefab; 
    private Sprite[,] allCardSprites = new Sprite[4, 13];

    [Header("Gameplay")]
    public int maxCardNum = 5;

    private List<Card> deck = new List<Card>(); // 전체 카드52장이 들어있는 덱
    private Dictionary<Image, Card> slotToCard = new Dictionary<Image, Card>();
    private HashSet<Image> selectedSlots = new HashSet<Image>(); 



    void Start()
    {
        // 카드 스프라이트 로드
        var loaded = Resources.LoadAll<Sprite>("Cards");
        BuildAllCardSprites(loaded);
        InitializeDeck();
        ShuffleDeck();
        DrawCard();
    }

    private void BuildAllCardSprites(Sprite[] sprites) //카드 스프라이트 배열을 2D 배열로 변환
    {
        // clear
        for (int s = 0; s < 4; s++) for (int n = 0; n < 13; n++) allCardSprites[s, n] = null;
        if (sprites == null) return;
        foreach (var sp in sprites)
        {
            if (sp == null || string.IsNullOrEmpty(sp.name)) continue;
            var name = sp.name.ToLower();
            var parts = name.Split('_');
            if (parts.Length < 2) continue;
            if (!int.TryParse(parts[1], out int num)) continue;// 파트를 2개 이상으로 나누고 두번째 파트가 숫자인지 확인
            if (num < 1 || num > 13) continue;
            // try parse suit
            try
            {
                var suit = (Card.Suit)System.Enum.Parse(typeof(Card.Suit), parts[0], true);
                int si = (int)suit;
                allCardSprites[si, num - 1] = sp;
            }
            catch { /* ignore unknown suit */ }
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        // number keys 1..9 and 0 map to indices 0..9
        if (Keyboard.current.digit1Key.wasPressedThisFrame) ToggleSelectSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) ToggleSelectSlot(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) ToggleSelectSlot(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) ToggleSelectSlot(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) ToggleSelectSlot(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) ToggleSelectSlot(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) ToggleSelectSlot(6);
        if (Keyboard.current.digit8Key.wasPressedThisFrame) ToggleSelectSlot(7);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) ToggleSelectSlot(8);
        if (Keyboard.current.digit0Key.wasPressedThisFrame) ToggleSelectSlot(9);

        // use selected cards with 'C'
        if (Keyboard.current.cKey.wasPressedThisFrame) UseSelectedCards();

        if (Keyboard.current.xKey.wasPressedThisFrame) DrawCard();
    }

    private void InitializeDeck()
    {
        deck.Clear();
        foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit)))
        {
            for (int number = 1; number <= 13; number++)
            {
                deck.Add(new Card(suit, number));
            }
        }

        ShuffleDeck();
    }


    private void ToggleSelectSlot(int index)
    {
        if (index < 0 || bottomCards == null || index >= bottomCards.Length) return;
        var slot = bottomCards[index];
        if (slot == null) return;
        if (slot.sprite == null || slot.sprite == backSprite) return; // nothing to select

        if (selectedSlots.Contains(slot))
        {
            // deselect (only UI state)
            selectedSlots.Remove(slot);
            SetSlotSelectionVisual(slot, false);
        }
        else
        {
            // limit selection by number of selected slots or maxCardNum
            if (selectedSlots.Count >= maxCardNum) return;
            // select (only UI state)
            selectedSlots.Add(slot);
            SetSlotSelectionVisual(slot, true);
        }
    }

    private void SetSlotSelectionVisual(Image slot, bool selected)
    {
        if (slot == null) return;
        slot.color = selected ? Color.yellow : Color.white;
    }

    // selectedSlots에 해당하는 Card 객체 리스트 반환
    private List<Card> GetSelectedCards()
    {
        var list = new List<Card>();
        foreach (var slot in selectedSlots)
        {
            if (slot == null) continue;
            if (slotToCard.TryGetValue(slot, out Card c))
            {
                list.Add(c);
            }
            else if (slot.sprite != null && !string.IsNullOrEmpty(slot.sprite.name))
            {
                if (Card.TryParseSpriteNameToCard(slot.sprite.name, out Card parsed)) list.Add(parsed);
            }
        }
        return list;
    }

    // evaluater테스트용 임시
    private void UseSelectedCards()
    {
        var currentSelected = GetSelectedCards();
        if (currentSelected == null || currentSelected.Count == 0)
        {
            Debug.Log("No selected cards to use.");
            return;
        }

        if (enemy == null)
        {
            Debug.LogWarning("Enemy GameObject not assigned on CardManager.");
            return;
        }

        // 적카드 수집
        var enemyImgs = enemy.GetComponentsInChildren<Image>(true);
        List<Card> enemyCards = new List<Card>();
        foreach (var img in enemyImgs)
        {
            if (img == null || img.sprite == null) continue;
            if (img.sprite == backSprite) continue;
            // try mapping from slotToCard first
            if (slotToCard.TryGetValue(img, out Card mapped))
            {
                enemyCards.Add(mapped);
            }
            else
            {
                if (Card.TryParseSpriteNameToCard(img.sprite.name, out Card parsed)) enemyCards.Add(parsed);
            }
        }

        // 1) 적카드 + 내카드
        var combined = new List<Card>();
        combined.AddRange(enemyCards);
        combined.AddRange(currentSelected);

        HandEvaluator.HandResult combinedRes = null;
        if (combined.Count >= 1 && combined.Count <= 5)
        {
            combinedRes = HandEvaluator.EvaluateHand(combined);
        }

        if (combinedRes != null && combinedRes.UsedAllCards)
        {
            // immediate victory when combined makes a full combination
            Debug.Log($"Victory: combined cards form a complete hand ({combinedRes.Name}). Applying effect to {enemy.name}.");
            // Placeholder effect
            Debug.Log($"ApplyEffectToEnemy: {enemy.name} because of {combinedRes.Name}");
            ClearSelectionsAfterUse();
            return;
        }

        // 2)내카드 
        var myRes = HandEvaluator.EvaluateHand(currentSelected);
        if (myRes == null || !myRes.UsedAllCards)
        {
            Debug.Log($"Defeat: selected cards do not form a complete hand. ({myRes?.Name ?? "None"})");
            ClearSelectionsAfterUse();
            return;
        }

        // 3) 비교
        var enemyRes = HandEvaluator.EvaluateHand(enemyCards);
        int cmp = CardComparer.Compare(myRes, enemyRes);
        if (cmp > 0)
        {
            Debug.Log($"Victory: my {myRes.Name} beats enemy {enemyRes?.Name ?? "None"}. Applying effect to {enemy.name}.");
            Debug.Log($"ApplyEffectToEnemy: {enemy.name} because of {myRes.Name}");
        }
        else
        {
            Debug.Log($"Defeat: my {myRes.Name} does not beat enemy {enemyRes?.Name ?? "None"}.");
        }

        ClearSelectionsAfterUse();
    }

    private void ClearSelectionsAfterUse()
    {
        // clear visuals and selected lists
        foreach (var slot in selectedSlots.ToList())
        {
            SetSlotSelectionVisual(slot, false);
        }
        // remove images from selected bottom slots
        foreach (var slot in selectedSlots.ToList())
        {
            ApplyCardToImage(slot, null);
            slotToCard.Remove(slot);
        }
        selectedSlots.Clear();
    }
    

    private void DrawCard()
    {
        // recreate bottom slots from card prefab under CardPanel and populate up to maxCardNum
        if (CardPanel == null || cardprefab == null)
        {
            Debug.LogWarning("CardPanel or cardprefab not assigned");
            return;
        }

        // clear existing child slots
        for (int i = CardPanel.transform.childCount - 1; i >= 0; i--)
        {
            var child = CardPanel.transform.GetChild(i).gameObject;
            Destroy(child);
        }

        int count = Mathf.Max(0, maxCardNum);
        bottomCards = new Image[count];
        for (int i = 0; i < count; i++)
        {
            Image inst = Instantiate(cardprefab, CardPanel.transform);
            inst.name = $"CardSlot_{i}";
            bottomCards[i] = inst;

            Card c = DrawNextCard();
            slotToCard[inst] = c;
            ApplyCardToImage(inst, c);
        }
    }

    private Card DrawNextCard()
    {
        if (deck.Count == 0)
        {
            InitializeDeck();
            ShuffleDeck();
        }

        if (deck.Count == 0) return null;
        Card c = deck[0];
        deck.RemoveAt(0);
        return c;
    }

    // Apply Card sprite (or back) to an UI Image. Centralized so visual adjustments stay in one place.
    public void ApplyCardToImage(Image img, Card card)
    {
        if (img == null) return;
        if (card == null)
        {
            img.sprite = backSprite;
            return;
        }
        var s = allCardSprites[(int)card.suit, Mathf.Clamp(card.number - 1, 0, 12)];
        img.sprite = s != null ? s : backSprite; //
    }

    private void ShuffleDeck()
    {
        deck = deck.OrderBy(_ => UnityEngine.Random.value).ToList();
    }
}
