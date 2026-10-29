using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GalleryController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ModelVisual preview;
    [SerializeField] private Button galleryPrefab;
    [SerializeField] private Transform contentParent;

    private int skinCount;
    private GameObject previousSelected;
    private List<GameObject> gallerys = new();


    /// <summary>
    /// アップデート処理
    /// </summary>
    public void GalleryUpdate()
    {
        // 現在選択中のオブジェクトを取得
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        // 選択中のオブジェクトの変更検知
        if (selected != null && selected != previousSelected)
        {
            RectTransform target = selected.GetComponent<RectTransform>();

            if (target != null && target.IsChildOf(scrollRect.content))
            {
                ScrollToTarget(target);
            }
        }
        // 現在選択中のオブジェクトがNULL or アクティブ状態ではないなら
        else if(selected == null && previousSelected.activeInHierarchy)
        {
            // 最後に選択していたボタンをセットする
            Selectable selectable = previousSelected.GetComponent<Selectable>();
            if (selectable != null && selectable.IsInteractable()) EventSystem.current.SetSelectedGameObject(previousSelected);
            selected = previousSelected;
        }

        // 選択中オブジェクトがギャラリーアイテムなら
        if(gallerys.Contains(selected)) previousSelected = selected;
    }

    /// <summary>
    /// 初期化 ギャラリーの設定
    /// </summary>
    public void Init()
    {
        // ギャラリー生成
        var skins = CharaDataManager.Instance.GetAllModelVisualData();

        // ギャラリー生成、バストアップ画像を設定
        SetGalleryData(galleryPrefab, skins[skinCount]);
        gallerys.Add(galleryPrefab.gameObject);
        skinCount++;

        // ギャラリーの生成
        for(; skinCount < skins.Length;)
            AddGallery(skins[skinCount]);
    }

    /// <summary>
    /// ギャラリーアイテムの追加
    /// </summary>
    /// <param name="data"></param>
    public void AddGallery(ModelVisualData data)
    {
        // ギャラリー生成
        var obj = Instantiate(galleryPrefab, contentParent);
        SetGalleryData(obj, data);
        gallerys.Add(obj.gameObject);

        skinCount++;
    }

    // バストアップ画像を設定、クリック時の処理を追加
    void SetGalleryData(Button obj, ModelVisualData data)
    {
        var kv = obj.transform.Find("CharaKV").GetComponent<Image>();
        kv.sprite = data.BustUp;

        int index = skinCount;
        // 押されたとき
        obj.onClick.AddListener(() => AttachVisual(index));
    }

    /// <summary>
    /// 指定番号のスキンをアタッチする
    /// </summary>
    /// <param name="index"></param>
    public void AttachVisual(int index)
    {
        var vs = CharaDataManager.Instance.GetModelVisualData(index);
        preview.AttachVisual(vs);
    }

    /// <summary>
    /// ターゲットアイテムをスクロールビューの中心に持ってくる
    /// </summary>
    /// <param name="target"></param>
    public void ScrollToTarget(RectTransform target)
    {
        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;

        // ボタンの中心をViewportのローカル座標に変換
        Vector3 worldCenter = target.TransformPoint(target.rect.center);
        Vector3 localCenter = viewport.InverseTransformPoint(worldCenter);

        // ボタンを中央にするための移動量
        float offset = localCenter.y - viewport.rect.center.y;

        Vector2 pos = content.anchoredPosition;
        pos.y -= offset;

        // Contentの高さとViewportの高さ
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        // スクロール可能な最大距離
        float maxScroll = Mathf.Max(0f, contentHeight - viewportHeight);

        // 上端・下端を超えないように制限
        pos.y = Mathf.Clamp(pos.y, 0f, maxScroll);

        content.anchoredPosition = pos;
    }
}
