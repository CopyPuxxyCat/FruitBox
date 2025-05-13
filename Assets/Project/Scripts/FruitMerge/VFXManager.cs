using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }
    [SerializeField] private GameObject mergeEffectPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayMergeEffect(Vector3 position)
    {
        Instantiate(mergeEffectPrefab, position, Quaternion.identity);
    }
}
