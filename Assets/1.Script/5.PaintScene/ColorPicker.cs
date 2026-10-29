using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カラーピッカーコントロールクラス
/// </summary>
public class ColorPicker : MonoBehaviour
{
    [Header("Preview")]
    [SerializeField] private Image colorPreview;

    [Header("SVHPicker")]
    [SerializeField] private SVPicker svhPicker;

    [Header("RGB Sliders(赤緑青)")]
    [SerializeField] private Slider rSlider;
    [SerializeField] private Slider gSlider;
    [SerializeField] private Slider bSlider;

    [Header("Hex(カラーコード)")]
    [SerializeField] private TMP_InputField hexInput;

    private bool updating;
    private Color currentColor = Color.black;

    /// <summary>
    /// 現在のカラーを返す
    /// </summary>
    public Color CurrentColor => currentColor;

    /// <summary>
    /// カラー変更されたときに呼ぶ処理
    /// </summary>
    public event Action<Color> OnColorChanged;


    public void Init(Color _color)
    {
        SetColor(_color);
    }

    public void SetColor(Color color)
    {
        if (updating) return;

        updating = true;

        currentColor = color;

        // SVH
        svhPicker.SetColor(color);

        // RGB
        rSlider.value = color.r;
        gSlider.value = color.g;
        bSlider.value = color.b;

        // Preview
        colorPreview.color = color;

        // Hex
        hexInput.text = ColorUtility.ToHtmlStringRGB(color);

        OnColorChanged?.Invoke(color);

        updating = false;
    }

    void OnRGBChanged()
    {
        if (updating) return;

        Color color = new Color(
            rSlider.value,
            gSlider.value,
            bSlider.value
        );

        SetColor(color);
    }

    // ===============
    // RGBスライダーで色を設定
    // ===============
    public void OnRedValueChanged(float _value)
    {
        rSlider.value = _value;
        OnRGBChanged();
    }
    public void OnGreenValueChanged(float _value)
    {
        gSlider.value = _value;
        OnRGBChanged();
    }
    public void OnBlueValueChanged(float _value)
    {
        bSlider.value = _value;
        OnRGBChanged();
    }

    /// <summary>
    /// カラーコードで色を設定する
    /// </summary>
    public void OnHexChanged()
    {
        if (updating) return;

        string hex = hexInput.text.Trim();

        // #があるか確認
        if (!hex.StartsWith("#")) hex = "#" + hex;

        // カラーコードをColorに変換
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            SetColor(color);
        }
    }

    /// <summary>
    /// HSVで色を設定する
    /// </summary>
    /// <param name="_hue">色相</param>
    /// <param name="_saturation">彩度</param>
    /// <param name="_value">明度</param>
    public void SetHSV(float _hue, float _saturation, float _value)
    {
        Color color = Color.HSVToRGB(
            _hue,
            _saturation,
            _value
        );

        SetColor(color);
    }
}
