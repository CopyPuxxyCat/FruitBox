using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FDNComboUI : MonoBehaviour
{
    [SerializeField] private List<Image> comboSlots; 
    [SerializeField] private Color normalColor = Color.gray;
    [SerializeField] private Color activeColor = Color.yellow;

    // Observer pattern: đăng ký ở GameManager
    public void UpdateComboUI(Sprite[] comboFruits, int activeIndex)
    {
        for (int i = 0; i < comboSlots.Count; i++)
        {
            if (i < comboFruits.Length)
            {
                comboSlots[i].enabled = true;
                comboSlots[i].sprite = comboFruits[i];

                // Đúng thứ tự thì sáng
                comboSlots[i].color = (i == activeIndex) ? activeColor : normalColor;
                LeanTween.scale(comboSlots[activeIndex].gameObject, Vector3.one * 1.2f, 0.2f).setLoopPingPong(1); 

            }
            else
            {
                comboSlots[i].enabled = false;
            }
        }
    }

    public void ResetComboUI()
    {
        foreach (var slot in comboSlots) slot.enabled = false;
    }
}
