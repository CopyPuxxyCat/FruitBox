using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FruitSliceCoroutineRunner : MonoBehaviour
{
    public static FruitSliceCoroutineRunner Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDelayedAction(float delay, System.Action action)
    {
        if (action != null && gameObject.activeInHierarchy)
        {
            StartCoroutine(DelayedActionCoroutine(delay, action));
        }
    }

    private IEnumerator DelayedActionCoroutine(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);

        if (action != null)
        {
            try
            {
                action.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in delayed action: {e.Message}");
            }
        }
    }

    // Method to stop all delayed actions (useful for cleanup)
    public void StopAllDelayedActions()
    {
        StopAllCoroutines();
    }
}