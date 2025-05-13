using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Fruit : MonoBehaviour, IFruit
{
    private FruitData _data;
    private ISkin _skin;

    public void Initialize(FruitData data, ISkin skin)
    {
        _data = data;
        _skin = skin;
        ApplySkin();
    }

    private void ApplySkin()
    {
        GetComponent<SpriteRenderer>().sprite = _skin.GetSprite(_data.Type);
    }

    public void Merge()
    {
        VFXManager.Instance.PlayMergeEffect(transform.position);
        SoundManager.Instance.PlaySFX(SFXType.Merge);
        FruitPool.Instance.ReturnToPool(this);
    }

    public FruitData GetData() => _data;
}

[CreateAssetMenu(menuName = "Fruit/FruitData")]
public class FruitData : ScriptableObject
{
    public FruitType Type;
    public GameObject Prefab;
    public float Scale;
}

public enum FruitType { Lime, Lemon, Cherry, Strawberry, Plum, Orange, Apple, StarFruit, Peach, Pear, Coconut, Watermelon }
