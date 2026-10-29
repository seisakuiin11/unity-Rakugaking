using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// <summary>
/// テクスチャの描画 (指定ピクセルに色を設定)
/// </summary>
[BurstCompile]
public struct DrawLineJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Color32> Pixels;

    public int TextureWidth;
    public int TextureHeight;

    public int MinX;
    public int MinY;
    public int RegionWidth;

    public float StartX;
    public float StartY;
    public float EndX;
    public float EndY;

    public float Radius;
    public Color32 Color;

    public void Execute(int index)
    {
        // 担当するピクセルの座標
        int x = MinX + index % RegionWidth; // 何列目
        int y = MinY + index / RegionWidth; // 何行目

        // 範囲外なら
        if (x < 0 || x >= TextureWidth || y < 0 || y >= TextureHeight)
            return;

        // 線分の方向ベクトル
        float dx = EndX - StartX;
        float dy = EndY - StartY;

        // 線分の長さ (2乗のまま)
        float lengthSq = dx * dx + dy * dy;

        // ピクセル中心から線分上の最近点を求める (内積)
        float t = 0f;

        if (lengthSq > 0f)
        {
            // 線分上のどの位置(割合)
            t = ((x - StartX) * dx + (y - StartY) * dy) / lengthSq;
            t = math.clamp(t, 0f, 1f); // 0%～100%内に収める
        }

        // 最近点
        float nearestX = StartX + dx * t;
        float nearestY = StartY + dy * t;

        // 線分と担当ピクセルとの距離
        float diffX = x - nearestX;
        float diffY = y - nearestY;

        // 線分からブラシ半径以上離れていたら描画しない
        if (diffX * diffX + diffY * diffY > Radius * Radius) return;

        // 描画
        Pixels[y * TextureWidth + x] = Color;
    }
}