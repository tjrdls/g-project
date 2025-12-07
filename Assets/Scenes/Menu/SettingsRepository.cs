using UnityEngine;

public class SettingsRepository
{
    private const string KEY = "GameSettings";

    public SettingsData Load()
    {
        if (!PlayerPrefs.HasKey(KEY))
        {
            return new SettingsData(); // ±âº»°ª
        }

        string json = PlayerPrefs.GetString(KEY);
        return JsonUtility.FromJson<SettingsData>(json);
    }

    public void Save(SettingsData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }
}
