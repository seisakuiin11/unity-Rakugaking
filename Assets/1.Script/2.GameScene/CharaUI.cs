using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// バトルシーンのキャラUI管理クラス
/// </summary>
public class CharaUI : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Image hpBarImage;
    [SerializeField] Slider hpBar;
    [Header("HPバー ダメージアニメーション設定")]
    [SerializeField] float AnimDurationTime = 0.3f;
    [SerializeField] float AnimStrength = 30f;
    [SerializeField] int AnimCount = 20;
    [SerializeField] float hpBarRedChangePercent=0.2f;
    Vector3 defaultPos;


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(int charaID, int maxValue)
    {
        image.sprite = CharaDataManager.Instance.GetVisualData(charaID).BustUp;
        hpBar.maxValue = maxValue;
        hpBar.value = maxValue;

        defaultPos = hpBar.transform.position;

        hpBarImage.color = Color.green;
    }

    public void ChangeHPValue(int value)
    {
        var hp = hpBar.value;

        hpBar.value = value;

        // HPが減っているなら、ダメージ演出
        if(hp > value)
        {
            hpBar.transform.DOKill();
            hpBar.transform.position = defaultPos;

            hpBar.transform.DOShakePosition(AnimDurationTime, AnimStrength, AnimCount);
        }

        // hpが一定割合以下の場合色を変える
        if (value / hpBar.maxValue <= hpBarRedChangePercent)
        {
            hpBarImage.color= Color.red;
        }
        else
        {
            hpBarImage.color= Color.green;
        }
    }
}
