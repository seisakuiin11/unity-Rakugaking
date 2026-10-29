using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PaintDirector : MonoBehaviour
{
    [Header("トランジション")]
    [SerializeField] private Animator hideTransition;
    [SerializeField, Tooltip("ミリ秒")] private int transitionTime;

    [Header("ギャラリー")]
    [SerializeField] private Animator galleryScene;
    [SerializeField] private GalleryController gallery;
    [SerializeField] private GameObject firstBtn;
    [SerializeField] private GameObject quitUI;
    [SerializeField] private GameObject quitUIBtn;

    [Header("らくがき")]
    [SerializeField] private GameObject paintScene;
    [SerializeField] private PaintManager paint;
    [SerializeField] private Animator popupUI;
    [SerializeField,Tooltip("ミリ秒")] private int popupDispTime;
    [SerializeField] private GameObject backUI;
    [SerializeField] private GameObject backUIBtn;
    [SerializeField] private EventSystem eventSystem;

    private bool openUI;
    private bool paintFlag;
    const int waitTime = 300;
    private bool waitFlag;


    void Start()
    {
        Init();
    }

    void Update()
    {
        if (openUI) return;

        // らくがき
        if (paintFlag) paint.HandleInput();
        // ギャラリー
        else gallery.GalleryUpdate();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    void Init()
    {
        SoundManager.Instance.BGMPlay(BGM.SELECT);

        // ギャラリーの初期化
        gallery.Init();

        // 初期化
        paint.Init();

        // 非表示
        paintScene.SetActive(false);
        quitUI.SetActive(false);
        backUI.SetActive(false);

        // 初期選択設定
        eventSystem.SetSelectedGameObject(firstBtn);

        // トランジションアニメーション
        hideTransition.gameObject.SetActive(true);
        hideTransition.SetTrigger("Hide");
        SoundManager.Instance.SEPlay(SE.MEKURU);
    }

    // ボタンを押した後、入力を数瞬無効にする
    void SetWaitFlag(int _waitTime)
    {
        waitFlag = true;
        Task.Run(async () => { await Task.Delay(_waitTime); waitFlag = false; });
    }

    /// <summary>
    /// 戻るUIの表示非表示
    /// </summary>
    /// <param name="_openUI"></param>
    public void OpenUI(bool _openUI)
    {
        if (waitFlag) return;

        SetWaitFlag(waitTime);

        openUI = _openUI;

        // ペイント中かで開くUIを変える
        if(paintFlag) backUI.SetActive(openUI);
        else quitUI.SetActive(openUI);

        // ペイント中かで選択するボタンを変える
        if (_openUI) {
            eventSystem.SetSelectedGameObject(paintFlag ? backUIBtn : quitUIBtn);
            SoundManager.Instance.SEPlay(SE.SHOW_WINDOW);
        }
        // UIを閉じるなら
        else {
            eventSystem.SetSelectedGameObject(null);
            SoundManager.Instance.SEPlay(SE.BACK);
        }
    }

    /// <summary>
    /// らくがきを始める
    /// </summary>
    public void StartPaint()
    {
        if (waitFlag) return;

        SetWaitFlag(transitionTime);

        // ペイントモード開始
        paintFlag = true;

        // コントローラー入力を無効化
        foreach (var gamepad in Gamepad.all)
        {
            InputSystem.DisableDevice(gamepad);
        }

        // ペイントシーンのリセット
        paint.ResetCanvas();

        // トランジション
        paintScene.SetActive(true);
        galleryScene.SetTrigger("Hide");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        DelaySetActive(galleryScene.gameObject, false);

        PopupUIDispAnim();
    }

    async void PopupUIDispAnim()
    {
        await Task.Delay(transitionTime);

        popupUI.gameObject.SetActive(true);
        popupUI.SetTrigger("Show");

        await Task.Delay(popupDispTime);

        popupUI.SetTrigger("Hide");
    }

    /// <summary>
    /// ギャラリーに戻る
    /// </summary>
    public void BackGallery()
    {
        if (waitFlag) return;

        SetWaitFlag(transitionTime);

        // ポップアップUIを消す
        openUI = false;
        backUI.SetActive(false);

        // ペイントモード終了
        paintFlag = false;

        // プレビューのリセット
        gallery.AttachVisual(0);

        // コントローラー入力の有効化
        foreach (var gamepad in Gamepad.all)
        {
            if (!gamepad.enabled)
                InputSystem.EnableDevice(gamepad);
        }

        // トランジション
        galleryScene.gameObject.SetActive(true);
        galleryScene.SetTrigger("Show");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        // 初期選択ボタン設定
        eventSystem.SetSelectedGameObject(firstBtn);

        DelaySetActive(paintScene, false);
    }

    // トランジションタイム後、SetActiveを呼ぶ
    async void DelaySetActive(GameObject obj, bool flag)
    {
        await Task.Delay(transitionTime);
        obj.SetActive(flag);
    }

    /// <summary>
    /// タイトルに戻る
    /// </summary>
    public　async void BackScene()
    {
        if(waitFlag) return;

        SetWaitFlag(transitionTime);

        // ポップアップUIの非表示
        quitUI.SetActive(false);

        // トランジション
        hideTransition.SetTrigger("Show");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        await Task.Delay(transitionTime);

        // タイトルシーンへ
        ProjectManager.Instance.ChangeScene(SceneName.TitleScene);
    }

    /// <summary>
    /// セーブ
    /// </summary>
    public void Save()
    {
        // セーブする
        var saveData = paint.Save();

        // セーブ
        CharaDataManager.Instance.SaveSkin(saveData);

        // ギャラリー生成
        gallery.AddGallery(saveData);

        // ギャラリーページに戻る
        BackGallery();
    }

    /*** コントローラー操作 ====================================================================== ***/
    public void OnMenu(InputAction.CallbackContext context)
    {
        if (waitFlag) return;

        openUI = !openUI;
        OpenUI(openUI);
    }
}
