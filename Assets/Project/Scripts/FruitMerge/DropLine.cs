using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropLine : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private Transform dropOrigin;
    [SerializeField] private LayerMask groundLayer;

    private void Update()
    {
        Vector2 origin = dropOrigin.position;
        Vector2 direction = Vector2.down;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 20f, groundLayer);

        line.SetPosition(0, origin);
        line.SetPosition(1, hit.collider != null ? hit.point : origin + direction * 20f);
    }
}

