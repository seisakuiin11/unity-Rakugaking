using System;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// ブラシの描画
/// </summary>
public class Brush : Graphic
{
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private float scaleUpValue = 1f;
    [SerializeField] private float thickness = 2f;
    [SerializeField] private int segments = 64;
    [SerializeField] private int minRadius = 1;
    [SerializeField] private int maxRadius = 300;
    [SerializeField] private Slider slider;

    private float radius = 10f;
    private Vector2 clickPos;


    /// <summary>
    /// ブラシサイズの変更時に呼ばれる
    /// </summary>
    public event Action<float> OnRadiusChanged;


    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="_radius"></param>
    public void Init(float _radius)
    {
        slider.minValue = minRadius;
        slider.maxValue = maxRadius;

        radius = _radius;
    }

    private void Update()
    {
        Vector2 localPos;

        // 右クリックした時
        if (Input.GetMouseButtonDown(1))
        {
            RadiusChangedStart();
        }
        // 右クリック押している間
        if(Input.GetMouseButton(1))
        {
            RadiusChanged();
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetRect,
            Input.mousePosition,
            null,
            out localPos
        );

        // ターゲットの範囲外なら
        if (localPos.x < targetRect.rect.xMin || localPos.x > targetRect.rect.xMax ||
            localPos.y < targetRect.rect.yMin || localPos.y > targetRect.rect.yMax)
            return;

        // 円の中心をカーソル位置へ
        rectTransform.anchoredPosition = localPos;

        // 描画
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // 外周白色
        DrawRing(vh, radius, thickness+1, Color.white);
        // 円
        DrawRing(vh, radius, thickness, color);
    }

    private void DrawRing(VertexHelper vh, float radius, float ringThickness, Color ringColor)
    {
        float outerRadius = radius + ringThickness / 2f;
        float innerRadius = radius - ringThickness / 2f;

        for (int i = 0; i < segments; i++)
        {
            float a0 = 2f * Mathf.PI * i / segments;
            float a1 = 2f * Mathf.PI * (i + 1) / segments;

            Vector2 outer0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * outerRadius;

            Vector2 outer1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * outerRadius;

            Vector2 inner0 = new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * innerRadius;

            Vector2 inner1 = new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * innerRadius;

            int index = vh.currentVertCount;

            vh.AddVert(outer0, ringColor, Vector2.zero);
            vh.AddVert(outer1, ringColor, Vector2.zero);
            vh.AddVert(inner0, ringColor, Vector2.zero);
            vh.AddVert(inner1, ringColor, Vector2.zero);

            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index + 1, index + 3, index + 2);
        }
    }

    // 開始位置を記録
    private void RadiusChangedStart()
    {
        clickPos = Input.mousePosition;
    }

    // ドラッグ距離からブラシサイズを変更する
    private void RadiusChanged()
    {
        // 移動量を取得
        float dif = Input.mousePosition.x - clickPos.x;

        // スケール変更
        radius += dif * scaleUpValue;
        if(radius < minRadius) radius = minRadius;
        if(radius > maxRadius) radius = maxRadius;
        SetVerticesDirty();

        slider.value = radius;

        OnRadiusChanged?.Invoke(radius);

        // ポジション更新
        clickPos = Input.mousePosition;
    }

    /// <summary>
    /// サイズ変更(スライダー)
    /// </summary>
    /// <param name="_radius"></param>
    public void RadiusChanged(float _radius)
    {
        radius = _radius;

        rectTransform.anchoredPosition = Vector2.zero;

        SetVerticesDirty();

        OnRadiusChanged?.Invoke(radius);
    }
}