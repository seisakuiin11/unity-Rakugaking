using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class TitleDirector : MonoBehaviour
{
    [SerializeField, Header("タイトル画面")] GameObject title;
    [SerializeField, Header("ゲームモード選択画面")] GameObject modeSelect;
    [SerializeField] GameObject[] modeBtns;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] Animator titleTransition;
    [SerializeField] Animator hideTransition;
    [SerializeField] int fadeTime;

    bool enterFlag;
    const int waitTime = 500;
    bool waitFlag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hideTransition.gameObject.SetActive(false);
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
        if (waitFlag || enterFlag) return;
        SetWaitFlag();

        titleTransition.SetTrigger("Hide");
        enterFlag = true;
        eventSystem.SetSelectedGameObject(modeBtns[0]);
    }

    /// <summary>
    /// タイトル画面に戻る
    /// </summary>
    public void BackTitle()
    {
        if (waitFlag) return;
        if (!enterFlag) { QuitGame(); return; }

        SetWaitFlag();

        titleTransition.SetTrigger("Show");
        enterFlag = false;
    }

    /// <summary>
    /// 次のシーンに移動する
    /// </summary>
    /// <param name="sceneName"></param>
    public async void NextScene(int index)
    {
        if (waitFlag) return;
        SetWaitFlag();

        hideTransition.gameObject.SetActive(true);
        hideTransition.SetTrigger("Show");

        await Task.Delay(fadeTime);

        Debug.Log((SceneName)index);
        ProjectManager.Instance.NextScene = (SceneName)index;
        ProjectManager.Instance.ChangeScene(SceneName.SelectScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルドしたアプリを終了する
        Application.Quit();
#endif
    }
}
