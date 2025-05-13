using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float radius = 2.5f;
    public LayerMask fruitLayer;

    public void Activate()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, fruitLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Fruit fruit))
            {
                fruit.Merge();
                GameManager.Instance.AddScore(10);
            }
        }
        SoundManager.Instance.PlaySFX(SFXType.Bomb);
    }
}
