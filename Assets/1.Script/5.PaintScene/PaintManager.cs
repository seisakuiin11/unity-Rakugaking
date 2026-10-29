using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Jobs;


/// <summary>
/// らくがきを管理するクラス
/// </summary>
public class PaintManager : MonoBehaviour
{
    const byte Front = 0, Back = 1;
    const int BU_Width = 400, BU_Height = 300; // バストアップ用の解像度

    public enum PaintMode : byte
    {
        Brush,
        Eraser
    }

    [System.Serializable]
    public struct Layer
    {
        public GameObject layer;
        public RawImage[] paintImages;
        [HideInInspector] public Texture2D[] paintTextures;
        public LinkedList<Color32[]>[] undoHistory;
    }

    [Header("Canvas")]
    [SerializeField] private int textureWidth = 500;
    [SerializeField] private int textureHeight = 500;
    [SerializeField] private int maxUndoCount = 100;
    [SerializeField] private Layer[] layers;

    [Header("Brush")]
    [SerializeField] private Color brushColor = Color.black;
    [SerializeField] private int brushSize = 12;
    [SerializeField] private Brush brush;

    [Header("Preview")]
    [SerializeField] private ModelVisual preview;

    [Header("ColorPicker")]
    [SerializeField] private ColorPicker colorPicker;

    [Header("ボタンs")]
    [SerializeField] private Image[] layerButtons;
    [SerializeField] private TextMeshProUGUI frontBackBtn_text;
    [SerializeField] private Image brushButton;
    [SerializeField] private Image eraserButton;

    private bool isPainting;
    private PaintMode paintMode;
    private byte layerType;
    private byte layerIndex;
    private int undoCount;
    private Vector2 previousPixelPosition;
    private RawImage paintImage;
    private Texture2D paintTexture;
    private LinkedList<byte> undoLayerHistory = new();
    private ModelVisualData saveData;


    // レイヤーのアタッチを簡易化
    private void OnValidate()
    {
        int layerCount = (int)ModelVisualData.LayerType.MAX;

        // 配列サイズを常に固定
        if (layers == null || layers.Length != layerCount)
        {
            System.Array.Resize(ref layers, layerCount);
        }

        for (int i = 0; i < layers.Length; i++)
        {
            GameObject parent = layers[i].layer;

            if (parent == null)
            {
                layers[i].paintImages = new RawImage[2];
                continue;
            }

            // 親の直下にある Front / Back を取得
            layers[i].paintImages = new RawImage[2];
            layers[i].paintImages[0] = parent.transform.Find("FrontPaintImage")?.GetComponent<RawImage>();
            layers[i].paintImages[1] = parent.transform.Find("BackPaintImage")?.GetComponent<RawImage>();
        }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        CreateCanvas();
        colorPicker.Init(brushColor);
        brush.Init(brushSize);

        colorPicker.OnColorChanged += SetColor;
        brush.OnRadiusChanged += SetBrushSize;

        // プレビュー用のアニメーションを再生する
        var anim = preview.transform.GetComponent<Animator>();
        anim.SetTrigger("Move");

        UseBrush();
    }

    private void OnDestroy()
    {
        colorPicker.OnColorChanged -= SetColor;
        brush.OnRadiusChanged -= SetBrushSize;

        // メモリの開放
        for(int i = 0; i < layers.Length; i++)
        {
            if (layers[i].paintTextures != null)
                foreach (var texture in layers[i].paintTextures) Destroy(texture);
            layers[i].paintTextures = null;

            if (layers[i].undoHistory != null)
                foreach (var history  in layers[i].undoHistory) history.Clear();
            layers[i].undoHistory = null;
        }
        paintTexture = null;

        undoLayerHistory.Clear();
    }

    // キャンバスの初期設定
    private void CreateCanvas()
    {
        // 各レイヤーの準備
        for(int i = 0; i < layers.Length; i++)
        {
            // テクスチャを用意
            layers[i].paintTextures = new Texture2D[2];
            layers[i].paintTextures[Front] = CreateTexture();
            layers[i].paintTextures[Back] = CreateTexture();
            // テクスチャをアタッチ
            layers[i].paintImages[Front].texture = layers[i].paintTextures[Front];
            layers[i].paintImages[Back].texture = layers[i].paintTextures[Back];
            layers[i].paintImages[Front].color = Color.white;
            layers[i].paintImages[Back].color = Color.white;
            // 履歴管理の準備
            layers[i].undoHistory = new LinkedList<Color32[]>[2];
            layers[i].undoHistory[Front] = new();
            layers[i].undoHistory[Back] = new();
        }

        layerType = (byte)ModelVisualData.LayerType.Body;
        layerIndex = Front;

        LayerChanged(layerType);
    }

    // テクスチャの用意
    private Texture2D CreateTexture()
    {
        Texture2D texture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            false
        );

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        ClearTexture(texture, Color.clear);

        return texture;
    }
    // テクスチャのリセット
    private void ClearTexture(Texture2D texture, Color32 color)
    {
        Color32[] pixels = new Color32[textureWidth * textureHeight];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;

        texture.SetPixels32(pixels);
        texture.Apply();
    }

    /// <summary>
    /// ペイント アップデート処理
    /// </summary>
    public void HandleInput()
    {
        // クリックしたとき ペイント開始
        if (Input.GetMouseButtonDown(0))
        {
            if (TryGetPixelPosition(Input.mousePosition, out Vector2 pixel))
            {
                SaveUndo(); // 記録

                isPainting = true;
                previousPixelPosition = pixel;

                // 描画
                DrawLine(pixel, pixel);
            }
        }

        // クリック中 ペイント中
        if (Input.GetMouseButton(0) && isPainting)
        {
            if (TryGetPixelPosition(Input.mousePosition, out Vector2 pixel))
            {
                // 描画
                DrawLine(previousPixelPosition, pixel);
                previousPixelPosition = pixel;
            }
        }

        // クリックを放したとき ペイント終了
        if (Input.GetMouseButtonUp(0))
        {
            isPainting = false;
        }

        // 巻き戻す
        if((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            && Input.GetKeyDown(KeyCode.Z)) Undo();
    }

    // paintImageのどの位置をクリックしたか
    private bool TryGetPixelPosition(Vector2 screenPosition, out Vector2 pixelPosition)
    {
        pixelPosition = Vector2.zero;

        RectTransform rectTransform = paintImage.rectTransform;

        // paintImageを原点としたカーソル位置
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screenPosition, null, out Vector2 localPosition))
        {
            return false;
        }

        Rect rect = rectTransform.rect;

        // ペイント中ではなくて、 PaintImageの範囲外なら終了
        if (!isPainting && !rect.Contains(localPosition)) return false;

        // 位置を0～1の割合位置に変換
        float x = Mathf.InverseLerp(rect.xMin, rect.xMax, localPosition.x);
        float y = Mathf.InverseLerp(rect.yMin, rect.yMax, localPosition.y);

        // ピクセル位置の設定
        pixelPosition = new Vector2(
            x * (textureWidth - 1),
            y * (textureHeight - 1)
        );

        return true;
    }

    // 線 描画
    private void DrawLine(Vector2 start, Vector2 end)
    {
        int radius = brushSize / 2;

        // テクスチャ全体ではなく、矩形ないだけの処理にとどめる
        // 線分全体を含む矩形(長方形) テクスチャ範囲内におさまっている左下頂点と右上頂点
        int minX = Mathf.Max( 0, Mathf.FloorToInt(Mathf.Min(start.x, end.x)) - radius );

        int maxX = Mathf.Min( textureWidth - 1, Mathf.CeilToInt(Mathf.Max(start.x, end.x)) + radius );

        int minY = Mathf.Max( 0, Mathf.FloorToInt(Mathf.Min(start.y, end.y)) - radius );

        int maxY = Mathf.Min( textureHeight - 1, Mathf.CeilToInt(Mathf.Max(start.y, end.y)) + radius );

        // 矩形の幅と高さ (始点も含めるため、+1)
        int regionWidth = maxX - minX + 1;
        int regionHeight = maxY - minY + 1;

        if (regionWidth <= 0 || regionHeight <= 0) return;

        // JobSystemにより並列化
        // Texture2D のピクセルデータを直接参照
        NativeArray<Color32> pixels = paintTexture.GetPixelData<Color32>(0);

        // Jobの用意
        DrawLineJob job = new DrawLineJob
        {
            // 実際のテクスチャデータ(Color32版)
            Pixels = pixels,
            // テクスチャ サイズ
            TextureWidth = textureWidth,
            TextureHeight = textureHeight,
            // 左下の頂点
            MinX = minX,
            MinY = minY,
            // 矩形の横幅
            RegionWidth = regionWidth,
            // 線分の始点と終点
            StartX = start.x,
            StartY = start.y,
            EndX = end.x,
            EndY = end.y,
            // ブラシ サイズ
            Radius = radius,
            Color = brushColor
        };

        // 矩形内の全ピクセル実行
        // トータル指定回数分、複数のワーカースレッドで実行
        JobHandle handle = job.Schedule( regionWidth * regionHeight, 64 );

        // 描画結果が出来上がるまで待つ
        handle.Complete();

        // GPU への反映
        paintTexture.Apply(false, false);
    }

    // 過去を記録 巻き戻しできるように
    private void SaveUndo()
    {
        // 記録可能個数を越えていたら、古い記録を削除
        if(undoCount >= maxUndoCount)
        {
            byte oldData = undoLayerHistory.Last.Value;

            int oldType = oldData / 10;
            int oldIndex = oldData % 10;

            layers[oldType].undoHistory[oldIndex].RemoveLast();
            undoLayerHistory.RemoveLast();
            undoCount--;
        }

        Color32[] pixels = paintTexture.GetPixels32();

        // 保存
        layers[layerType].undoHistory[layerIndex].AddFirst(pixels);
        byte index = (byte)(layerType * 10 + layerIndex);
        undoLayerHistory.AddFirst(index);

        undoCount++;
    }

    /// <summary>
    /// 巻き戻し
    /// </summary>
    public void Undo()
    {
        // 描画中なら
        if (isPainting) return;

        if (undoLayerHistory.Count == 0)
            return;

        // 抽出
        var num = undoLayerHistory.First.Value;
        int type = num / 10;
        int index = num % 10;
        Color32[] pixels = layers[type].undoHistory[index].First.Value;

        // 更新
        var tex = layers[type].paintTextures[index];
        tex.SetPixels32(pixels);
        tex.Apply();

        // 記録の削除
        undoLayerHistory.RemoveFirst();
        layers[type].undoHistory[index].RemoveFirst();

        undoCount--;
    }

    /// <summary>
    /// カラーの設定
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(Color color)
    {
        brushColor = color;

        // 消しゴムを使用しているなら
        if (paintMode == PaintMode.Eraser) UseBrush();
    }

    /// <summary>
    /// ブラシサイズの設定
    /// </summary>
    /// <param name="size"></param>
    public void SetBrushSize(float size)
    {
        brushSize = Mathf.RoundToInt(size);
    }

    /// <summary>
    /// 消しゴムモード
    /// </summary>
    public void UseEraser()
    {
        paintMode = PaintMode.Eraser;

        brushColor = Color.clear;
        brushButton.color = Color.white;
        eraserButton.color = Color.cyan;
    }

    /// <summary>
    /// ペイントモード
    /// </summary>
    public void UseBrush()
    {
        paintMode = PaintMode.Brush;

        brushColor = colorPicker.CurrentColor;
        brushButton.color = Color.cyan;
        eraserButton.color = Color.white;
    }

    /// <summary>
    /// レイヤーを切り替える
    /// </summary>
    /// <param name="layer"></param>
    public void LayerChanged(int layer)
    {
        // 落書きする部位を変える
        layerType = (byte)layer;
        paintImage = layers[layer].paintImages[layerIndex];
        paintTexture = layers[layer].paintTextures[layerIndex];

        // どの部位を選択しているかハイライトする
        foreach (var btn in layerButtons) btn.color = Color.white;
        layerButtons[layer].color = Color.yellow;

        // 落書き中のレイヤーを前面にし、ハイライト
        foreach(var ly in layers)
        {
            // 半透明にする
            ly.paintImages[Front].color = new Color(1f, 1f, 1f, 0.5f);
            ly.paintImages[Back].color = new Color(1f, 1f, 1f, 0.5f);
        }
        // ハイライト
        layers[layer].paintImages[Front].color = Color.white;
        layers[layer].paintImages[Back].color = Color.white;
    }

    /// <summary>
    /// 前後のレイヤーを切り替える
    /// </summary>
    public void FrontBackChanged()
    {
        layerIndex = layerIndex == Front ? Back : Front;
        paintImage = layers[layerType].paintImages[layerIndex];
        paintTexture = layers[layerType].paintTextures[layerIndex];

        if (layerIndex == Front)
        {
            frontBackBtn_text.text = "前面レイヤー\r\n  →背面レイヤー";
        }

        if(layerIndex == Back)
        {
            frontBackBtn_text.text = "背面レイヤー\r\n  →前面レイヤー";
        }
    }

    /// <summary>
    /// Spriteの生成
    /// </summary>
    public void ApplyToPlayer()
    {
        // 保存用の入れ物を用意
        var data = ScriptableObject.CreateInstance<ModelVisualData>();

        // 全身合成用のピクセル配列
        Color32[] allPixels = new Color32[textureWidth * textureHeight];

        // 各レイヤーを重ねていく
        for (int i = 0; i < layers.Length; i++)
        {
            // 2レイヤーを1枚に合成する
            Texture2D resultTexture = new Texture2D(
                    textureWidth,
                    textureHeight,
                    TextureFormat.RGBA32,
                    false);

            var resultPixels = MixTexure(layers[i]);

            resultTexture.SetPixels32(resultPixels);
            resultTexture.Apply();

            Sprite sp = Sprite.Create(
                resultTexture,
                new Rect(0, 0, paintTexture.width, paintTexture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );
            sp.name = "CustomPlayerSprite_" + (ModelVisualData.LayerType)i;

            // データを保存
            data.partSprite[i] = sp;

            // 全身用に各部位を合成
            for (int p = 0; p < allPixels.Length; p++)
            {
                // 透明ピクセルは無視
                if (resultPixels[p].a == 0)
                    continue;

                // 後から処理した部位の色を優先
                allPixels[p] = resultPixels[p];
            }
        }

        // 全身用Textureを生成
        Texture2D allTexture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            false);

        allTexture.SetPixels32(allPixels);
        allTexture.Apply();

        // 全身用Spriteを生成
        Sprite allSprite = Sprite.Create(
            allTexture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f),
            100f);

        allSprite.name = "CustomPlayerSprite_All";

        // 全身用Spriteを保存
        data.KeyVisual = allSprite;
        // バストアップSpriteを保存
        data.BustUp = CreateBustUpSprite(allSprite);

        // プレビュー用にアタッチ
        preview.AttachVisual(data);

        saveData = data;
    }

    // 前後レイヤーを合体させる
    Color32[] MixTexure(Layer layer)
    {
        Color32[] resultPixels = new Color32[textureWidth * textureHeight];

        // 後面レイヤー
        Color32[] pixels = layer.paintTextures[Back].GetPixels32();
        // ピクセルごとに抽出
        for (int p = 0; p < resultPixels.Length; p++)
        {
            if (pixels[p] == Color.clear) continue;

            resultPixels[p] = pixels[p];
        }

        // 前面
        pixels = layer.paintTextures[Front].GetPixels32();
        // ピクセルごとに抽出
        for (int p = 0; p < resultPixels.Length; p++)
        {
            if (pixels[p] == Color.clear) continue;

            resultPixels[p] = pixels[p];
        }

        return resultPixels;
    }

    // バストアップのSpriteを書き出し
    Sprite CreateBustUpSprite(Sprite _sprite)
    {
        if (_sprite == null)
        {
            Debug.LogWarning("allSpriteが設定されていません。");
            return null;
        }

        Texture2D sourceTexture = _sprite.texture;

        int width = BU_Width;
        int height = BU_Height;

        // 元画像の中央を切り出す座標
        int x = (sourceTexture.width - width) / 2;
        int y = sourceTexture.height - height;

        // 切り出したピクセルを取得
        Color[] pixels = sourceTexture.GetPixels(
            (int)_sprite.rect.x + x,
            (int)_sprite.rect.y + y,
            width,
            height
        );

        // 新しいテクスチャを作成
        Texture2D croppedTexture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false
        );

        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        // Spriteを作成
        Sprite croppedSprite = Sprite.Create(
            croppedTexture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            _sprite.pixelsPerUnit
        );

        croppedSprite.name = "CustomPlayerSprite_BustUp";

        return croppedSprite;
    }

    /// <summary>
    /// スキンを保存する
    /// </summary>
    public ModelVisualData Save()
    {
        // 現時点での画像を保存
        ApplyToPlayer();

        return saveData;
    }

    /// <summary>
    /// キャンバスのリセット
    /// </summary>
    public void ResetCanvas()
    {
        // テクスチャのリセット
        for(int i = 0; i < layers.Length; i++)
        {
            // テクスチャをクリア
            ClearTexture(layers[i].paintTextures[Front], Color.clear);
            ClearTexture(layers[i].paintTextures[Back], Color.clear);

            // 履歴の削除
            layers[i].undoHistory[Front].Clear();
            layers[i].undoHistory[Back].Clear();
        }

        // レイヤーのリセット
        layerType = (byte)ModelVisualData.LayerType.Body;
        if(layerIndex == Back) FrontBackChanged();
        LayerChanged(layerType);

        // モデルを元に戻す
        var skin = CharaDataManager.Instance.GetModelVisualData(0);
        preview.AttachVisual(skin);
    }
}
