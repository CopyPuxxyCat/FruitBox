using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageSelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;

    private void Start()
    {
        // Set dropdown to match current locale when game starts
        var currentLocale = LocalizationSettings.SelectedLocale.Identifier.Code;
        int index = currentLocale == "en" ? 0 : currentLocale == "vi-VN" ? 1 : 0;
        languageDropdown.value = index;
        languageDropdown.RefreshShownValue();

        // Add listener
        languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    public void OnDropdownValueChanged(int index)
    {
        switch (index)
        {
            case 0: // English
                SetLocale("en");
                break;
            case 1: // Vietnamese
                SetLocale("vi-VN");
                break;
        }
    }

    private void SetLocale(string code)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        foreach (var locale in locales)
        {
            if (locale.Identifier.Code == code)
            {
                LocalizationSettings.SelectedLocale = locale;
                break;
            }
        }
    }
}

