using System.Collections.Generic;
using UnityEngine;

public class FruitPool : MonoBehaviour
{
    public static FruitPool Instance { get; private set; }

    private Dictionary<FruitType, Queue<Fruit>> poolDictionary = new();
    [SerializeField] private List<FruitData> fruitDefinitions;

    private void Awake()
    {
        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var fruit in fruitDefinitions)
        {
            var queue = new Queue<Fruit>();
            int count = GetInitialCount(fruit.Type);
            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(fruit.Prefab).GetComponent<Fruit>();
                obj.gameObject.SetActive(false);
                queue.Enqueue(obj);
            }
            poolDictionary.Add(fruit.Type, queue);
        }
    }

    private int GetInitialCount(FruitType type)
    {
        int index = (int)type;
        if (index <= 5) return 10;
        else if (index <= 9) return 6;
        return 4;
    }

    public Fruit GetFruit(FruitType type)
    {
        var fruit = poolDictionary[type].Dequeue();
        fruit.gameObject.SetActive(true);
        poolDictionary[type].Enqueue(fruit);
        return fruit;
    }

    public void ReturnToPool(Fruit fruit)
    {
        fruit.gameObject.SetActive(false);
    }
}
