using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class TitleDirector : MonoBehaviour
{
    [SerializeField, Header("タイトル画面")] GameObject title;
    [SerializeField, Header("ゲームモード選択画面")] GameObject modeSelect;
    [SerializeField] GameObject quitUI;
    [SerializeField] GameObject quitUI_btn;
    [SerializeField] GameObject[] modeBtns;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] Animator titleTransition;
    [SerializeField] Animator hideTransition;
    [SerializeField] int fadeTime;

    bool enterFlag;
    bool openQuitUI;
    const int waitTime = 500;
    bool waitFlag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // BGM再生
        SoundManager.Instance.BGMPlay(BGM.TITLE);

        SetWaitFlag();

        quitUI.SetActive(false);
        titleTransition.gameObject.SetActive(true);
        hideTransition.gameObject.SetActive(true);
        hideTransition.SetTrigger("Hide");
        SoundManager.Instance.SEPlay(SE.MEKURU);
    }

    // コントローラー起因処理 ===============================================================

    // ボタンを押した後、入力を数瞬無効にする
    public void SetWaitFlag()
    {
        waitFlag = true;
        Task.Run(async () => { await Task.Delay(waitTime); waitFlag = false; });
    }

    /// <summary>
    /// ゲームモード選択画面に進む
    /// </summary>
    public void EnterTitle()
    {
        if (waitFlag || enterFlag || openQuitUI) return;
        SetWaitFlag();

        titleTransition.SetTrigger("Hide");
        enterFlag = true;
        eventSystem.SetSelectedGameObject(modeBtns[0]);
        SoundManager.Instance.SEPlay(SE.TITLE_ENTER);
        SoundManager.Instance.SEPlay(SE.MEKURU);
    }

    /// <summary>
    /// タイトル画面に戻る
    /// </summary>
    public void BackTitle()
    {
        if (waitFlag) return;

        // タイトルなら、ゲーム終了確認UIを表示,非表示
        if (!enterFlag) { SetActiveQuitUI(!openQuitUI); return; }

        SetWaitFlag();

        titleTransition.SetTrigger("Show");
        enterFlag = false;
        SoundManager.Instance.SEPlay(SE.MEKURU);
    }

    /// <summary>
    /// 次のシーンに移動する
    /// </summary>
    /// <param name="sceneName"></param>
    public async void NextScene(string sceneName)
    {
        if (waitFlag) return;
        SetWaitFlag();

        // enum変換
        if(!Enum.TryParse<SceneName>(sceneName, out var scene)) { Debug.LogError("SceneNameが違います"); return; }

        // アニメーション再生
        hideTransition.gameObject.SetActive(true);
        hideTransition.SetTrigger("Show");
        SoundManager.Instance.SEPlay(SE.MEKURU);

        await Task.Delay(fadeTime);

        Debug.Log(scene);
        ProjectManager.Instance.NextScene = scene;

        // チュートリアルなら、キャラ選択へいかない
        if (scene == SceneName.TutorialScene) ProjectManager.Instance.ChangeScene(scene);
        else ProjectManager.Instance.ChangeScene(SceneName.SelectScene);
    }

    /// <summary>
    /// ゲーム終了確認UIの表示,非表示
    /// </summary>
    /// <param name="active"></param>
    public void SetActiveQuitUI(bool active)
    {
        if (waitFlag) return;
        SetWaitFlag();

        openQuitUI = active;
        quitUI.SetActive(openQuitUI);

        // SE再生
        if (active) SoundManager.Instance.SEPlay(SE.SHOW_WINDOW);
        else SoundManager.Instance.SEPlay(SE.BACK);

        if (!active) return;

        eventSystem.SetSelectedGameObject(quitUI_btn);
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    public void QuitGame()
    {
        SetActiveQuitUI(false);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルドしたアプリを終了する
        Application.Quit();
#endif
    }
}
