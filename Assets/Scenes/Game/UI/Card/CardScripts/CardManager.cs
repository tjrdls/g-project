using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


//카드 리소스 경로: Resources/Cards 폴더
//HandEvaluator.cs에서 족보 평가
//카드 리소스 이름 규칙: "suit_number" (예: "spade_1", "heart_13")
public class CardManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject CardPanel;
    private Image[] bottomCards; // 카드 UI 슬롯
    public Sprite backSprite;   // 카드 뒷면
    public Image cardprefab;
    public Image enemyCardPrefab; // 더 작은 프리팹
    private Sprite[,] allCardSprites = new Sprite[4, 13];

    [Header("Gameplay")]
    public int maxCardNum = 5;

    private List<Card> deck = new List<Card>(); // 전체 카드52장이 들어있는 덱
    private Dictionary<Image, Card> slotToCard = new Dictionary<Image, Card>();
    private HashSet<Image> selectedSlots = new HashSet<Image>();
    private List<Card> lastUsedCards = new List<Card>();

    private PlayerMovementClass playerMovement; //플레이어 필드와 연결 *(추가)
    private PlayerInputActions playerInputActions;

    public static CardManager instance;

    void Start()
    {
        instance = this;
        // 카드 스프라이트 로드
        var loaded = Resources.LoadAll<Sprite>("Cards");
        //플레이어 연결
        //playerMovement = FindObjectOfType<PlayerMovementClass>();
        playerMovement = FindFirstObjectByType<PlayerMovementClass>();

        try {             
            playerInputActions = InputManager.Instance.inputActions;
        } catch {
            Debug.LogWarning("InputManager or PlayerInputActions not found. Defaulting to keyboard input.");
        }

        BuildAllCardSprites(loaded);
        InitializeDeck();
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
        if (playerInputActions != null)
        {
            if (playerInputActions.Player.Skill.triggered) //플레이어가 스킬 사용 가능한 상태인지
            {
                if (playerMovement == null)
                    playerMovement = FindAnyObjectByType<PlayerMovementClass>();

                if (playerMovement != null && !playerMovement.SKillCheck())
                {
                    Debug.Log("Skill blocked: Player cannot use skill now.");
                    return;  // 카드 사용 막음
                }

                UseSelectedCards();
            }
        } else {
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                if (playerMovement == null)
                    playerMovement = FindAnyObjectByType<PlayerMovementClass>();
                if (playerMovement != null && !playerMovement.SKillCheck())
                {
                    Debug.Log("Skill blocked: Player cannot use skill now.");
                    return;  // 카드 사용 막음
                }

                UseSelectedCards();
            }
        }



        if (Keyboard.current.rKey.wasPressedThisFrame) DrawCard();
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

    private void UseSelectedCards()
    {
        var currentSelected = GetSelectedCards();
        if (currentSelected == null || currentSelected.Count == 0)
        {
            Debug.Log("No selected cards to use.");
            return;
        }

        //카드 선택되어 있을 시에 플레이어 스킬 사용
        playerMovement.TrySkill();


        ClearSelectionsAfterUse();
    }

    // 승자 결정 메서드 (다른 클래스에서 호출하면 모든 적들에 대해 승패 판정후 statManager에 카드 효과 적용 요청)
    public void DetermineWinner(GameObject enemy)
    {
        Debug.Log($"Determining winner against enemy: {enemy.name}");
        // 적카드 수집
        GameObject cardPanel = enemy.GetComponent<Enemy>()?.cardPanel;
        List<Card> enemyCards = new List<Card>();
        // 카드 패널 아래에서 Image 컴포넌트들 가져오기
        if (cardPanel != null) {
            var enemyImgs = cardPanel.GetComponentsInChildren<Image>(true);
            foreach (var img in enemyImgs)
            {
                if (img == null || img.sprite == null) continue;
                if (img.sprite == backSprite) continue;
                if (Card.TryParseSpriteNameToCard(img.sprite.name, out Card parsed)) enemyCards.Add(parsed);
                
            }
        }
        Debug.Log($"Enemy {enemy.name} has {enemyCards.Count} cards.");

        // 1) 적카드 + 내카드
        var combined = new List<Card>();
        combined.AddRange(enemyCards);
        combined.AddRange(lastUsedCards);

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
            applyEfect(enemy, combinedRes.Rank);
            return;
        }

        // 2)내카드 
        var myRes = HandEvaluator.EvaluateHand(lastUsedCards);
        if (myRes == null || !myRes.UsedAllCards)
        {
            Debug.Log($"Defeat: selected cards do not form a complete hand. ({myRes?.Name ?? "None"})");
            return;
        }

        // 3) 비교
        var enemyRes = HandEvaluator.EvaluateHand(enemyCards);
        int cmp = CardComparer.Compare(myRes, enemyRes);
        if (cmp > 0)
        {
            Debug.Log($"Victory: my {myRes.Name} beats enemy {enemyRes?.Name ?? "None"}. Applying effect to {enemy.name}.");
            Debug.Log($"ApplyEffectToEnemy: {enemy.name} because of {myRes.Name}");
            applyEfect(enemy, myRes.Rank);
        }
        else
        {
            Debug.Log($"Defeat: my {myRes.Name} does not beat enemy {enemyRes?.Name ?? "None"}.");
        }


    }

    static void applyEfect(GameObject enemy, HandEvaluator.HandRank handRank)
    {
        enemy.GetComponent<StatsComponent>()?.applyEffect(handRank);
    }
    private void ClearSelectionsAfterUse()
    {
        // store last used cards
        lastUsedCards = GetSelectedCards();

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


    private void DrawCard()// bottomCards 슬롯에 카드 배치
    {
        // maxCardNum만큼 덱에서 카드를 뽑아 카드 프리팹으로 만든 슬롯에 배치
        if (CardPanel == null || cardprefab == null)
        {
            Debug.LogWarning("CardPanel or cardprefab not assigned");
            return;
        }

        // 카드 패널 비우기
        for (int i = CardPanel.transform.childCount - 1; i >= 0; i--)
        {
            var child = CardPanel.transform.GetChild(i).gameObject;
            Destroy(child);
        }

        bottomCards = new Image[maxCardNum];
        for (int i = 0; i < maxCardNum; i++)
        {
            Image inst = Instantiate(cardprefab, CardPanel.transform);
            inst.name = $"CardSlot_{i}";
            bottomCards[i] = inst;

            //deck의 첫번째 요소를 뽑고, deck의 마지막으로 이동
            Card c = deck[0];
            deck.RemoveAt(0);
            deck.Add(c);

            slotToCard[inst] = c;
            ApplyCardToImage(inst, c);
        }

        ShuffleDeck();
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

    public void enemyCardDraw(GameObject cardPanel, List<Card> cards) 
    {
        foreach (var card in cards)
        {
            Image inst = Instantiate(enemyCardPrefab, cardPanel.transform);
            ApplyCardToImage(inst, card);
        }
    }
}
