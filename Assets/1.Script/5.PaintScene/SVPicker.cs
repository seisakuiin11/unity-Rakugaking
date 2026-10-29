using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/// <summary>
/// SVピッカー (彩度,明度調整用) □から色を取るやつ
/// </summary>
public class SVPicker : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("UI")]
    [SerializeField] private RawImage rawImage;
    [SerializeField] private RectTransform cursor;
    [SerializeField] private Slider hueSlider;

    [Header("Texture")]
    [SerializeField] private int textureSize = 256;

    [Header("Color Picker")]
    [SerializeField] private ColorPicker colorPicker;

    private Texture2D svTexture;
    private RectTransform rectTransform;

    private float hue = 0f;         // 色相
    private float saturation = 0f;  // 彩度
    private float value = 0f;       // 明度


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        CreateTexture();
    }

    /// <summary>
    /// テクスチャの生成
    /// </summary>
    private void CreateTexture()
    {
        // Texture2Dを1回だけ生成
        svTexture = new Texture2D(
            textureSize,
            textureSize,
            TextureFormat.RGBA32,
            false
        );

        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.filterMode = FilterMode.Bilinear;

        rawImage.texture = svTexture;

        // 初期Hueでテクスチャ生成
        UpdateTexture();
    }

    /// <summary>
    /// Hue(色相)を変更する (スライダー用)
    /// </summary>
    public void SetHue(float _hue)
    {
        _hue = Mathf.Clamp01(_hue);

        // 同じHueなら何もしない (ほぼ等しい)
        if (Mathf.Approximately(hue, _hue)) return;

        hue = _hue;

        // Hueが変わったときだけテクスチャ更新
        UpdateTexture();

        // 現在のS/Vを維持したまま色を更新
        UpdateColor();
    }

    /// <summary>
    /// SVテクスチャを更新
    /// </summary>
    private void UpdateTexture()
    {
        Color[] pixels = new Color[textureSize * textureSize];

        // 各ピクセルの色を設定
        for (int y = 0; y < textureSize; y++)
        {
            // 下 = 0 , 上 = 1
            // 明度
            float v = (float)y / (textureSize - 1);

            for (int x = 0; x < textureSize; x++)
            {
                // 左 = 0 , 右 = 1
                // 彩度
                float s = (float)x / (textureSize - 1);

                // 色設定
                pixels[y * textureSize + x] = Color.HSVToRGB(hue, s, v);
            }
        }

        svTexture.SetPixels(pixels);
        svTexture.Apply();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateSVPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateSVPosition(eventData);
    }

    /// <summary>
    /// カーソル位置から、SV(彩度,明度)の更新
    /// </summary>
    /// <param name="eventData"></param>
    private void UpdateSVPosition(PointerEventData eventData)
    {
        // SVピッカーを基準としたポジションへ変換
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            return;
        }

        Rect rect = rectTransform.rect;

        // 彩度,明度の設定
        saturation = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        value = Mathf.InverseLerp(rect.yMin, rect.yMax,localPoint.y);

        // 0～1の間に成形
        saturation = Mathf.Clamp01(saturation);
        value = Mathf.Clamp01(value);

        // カーソル位置更新 , 色更新
        UpdateCursor();
        UpdateColor();
    }

    /// <summary>
    /// カーソル位置の更新
    /// </summary>
    private void UpdateCursor()
    {
        if (cursor == null) { Debug.LogError("カーソルをアタッチしてください"); return; }

        Rect rect = rectTransform.rect;

        // 座標設定
        float x = Mathf.Lerp(rect.xMin, rect.xMax, saturation);
        float y = Mathf.Lerp(rect.yMin, rect.yMax, value);

        cursor.anchoredPosition = new Vector2(x, y);
    }

    /// <summary>
    /// 色変更
    /// </summary>
    private void UpdateColor()
    {
        if (colorPicker == null) { Debug.LogError("ColorPickerをアタッチしてください"); return; }

        // 色変更
        colorPicker.SetHSV(hue, saturation, value);
    }

    public void SetColor(Color color)
    {
        // HSVに変換
        Color.RGBToHSV(color, out var _hue, out var _saturation, out var _value);

        // hueを更新するか
        if(_value != 0) hue = _hue;

        if (_value != 0) saturation = _saturation;

        value = _value;

        // カーソル位置更新
        UpdateCursor();
        hueSlider.value = hue;

        // 色変更
        UpdateTexture();
    }

    private void OnDestroy()
    {
        if (svTexture != null)
        {
            Destroy(svTexture);
        }
    }
}
