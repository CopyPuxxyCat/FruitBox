using UnityEngine;

public interface IFruit
{
    void Initialize(FruitData data, ISkin skin);
    void Merge();
    FruitData GetData();
}
