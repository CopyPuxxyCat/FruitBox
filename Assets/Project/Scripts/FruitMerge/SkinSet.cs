using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fruit/SkinSet")]
public class SkinSet : ScriptableObject, ISkin
{
    public List<SkinEntry> Skins;

    public Sprite GetSprite(FruitType type)
    {
        return Skins.Find(x => x.Type == type).Sprite;
    }
}

[System.Serializable]
public class SkinEntry
{
    public FruitType Type;
    public Sprite Sprite;
}

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }
    public List<SkinSet> availableSets;
    public ISkin CurrentSkin { get; private set; }
    private int selectedIndex;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        LoadSkin();
    }

    public void SelectSkin(int index)
    {
        selectedIndex = index;
        CurrentSkin = availableSets[index];
        PlayerPrefs.SetInt("skinIndex", index);
    }

    private void LoadSkin()
    {
        selectedIndex = PlayerPrefs.GetInt("skinIndex", 0);
        CurrentSkin = availableSets[selectedIndex];
    }
}

