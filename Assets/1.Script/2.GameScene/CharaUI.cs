using UnityEngine;
using UnityEngine.UI;

public class CharaUI : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Slider hpBar;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(Sprite _img, int maxValue)
    {
        image.sprite = _img;
        hpBar.maxValue = maxValue;
        hpBar.value = maxValue;
    }

    public void ChangeHPValue(int value)
    {
        hpBar.value = value; 
    }
}
