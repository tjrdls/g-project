using UnityEngine;
using UnityEditor;

public class CardSpriteRenamer : AssetPostprocessor
{
    void OnPostprocessTexture(Texture2D texture)
    {
        if (!assetPath.EndsWith("Resources/cards/cards.png")) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        if (importer.spriteImportMode != SpriteImportMode.Multiple) return;

        string[] suits = { "spade", "diamond", "club", "heart" };
        string[] values = { "1", "4", "7", "10", "K", "2", "5", "8", "J", "3", "6", "9", "Q" }; // 조커 제외

        var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath);

        int suitIndex = 0;
        int valueIndex = 0;

        foreach (var s in sprites)
        {
            if (!(s is Sprite sprite)) continue;

            // 조커 처리: 스프라이트가 black_joker 또는 color_joker 위치라면 별도로 지정
            if (sprite.name.Contains("black_joker") || sprite.name.Contains("color_joker"))
            {
                sprite.name = sprite.name; // 이미 이름이 맞다면 그대로
                continue;
            }

            sprite.name = $"{suits[suitIndex]}_{values[valueIndex]}";

            // 다음 카드로 이동
            suitIndex = (suitIndex + 1) % suits.Length;
            if (suitIndex == 0)
            {
                valueIndex++;
                if (valueIndex >= values.Length) break;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Cards.png sprite names updated!");
    }
}
