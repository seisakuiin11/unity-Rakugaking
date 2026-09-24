using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// ゲーム中のUIを管理
/// </summary>
public class GameUIController : MonoBehaviour
{
    [SerializeField] CharaUI[] charaUIs;
    [SerializeField] float charaUIDisSpace;
    [SerializeField] GameObject pauseUI;
    [SerializeField] GameObject pauseUI_btn;
    [SerializeField] EventSystem eventSystem;

    [SerializeField] GameObject[] playerNumbers;
    [SerializeField] float playerNumberShiftPosY;
    [SerializeField] CharaVisualData[] charaVisualDatas;

    [SerializeField, Header("カウントダウンテキスト")]
    TextMeshProUGUI countDownText;

    private void Awake()
    {
        if(countDownText != null)
            countDownText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(CharacterController[] _characters, int maxPlayer)
    {
        // 一度非表示
        if (charaUIs != null)
            foreach (var cu in charaUIs) cu.gameObject.SetActive(false);
        if (playerNumbers != null)
            foreach (var pn in playerNumbers) pn.SetActive(false);

        // キャラアイコン 生成
        int count = 0;
        var harfWidth = (maxPlayer - 1) * charaUIDisSpace * 0.5f;
        var charaIDs = ProjectManager.Instance.GetCharaIDs();
        for(int i = 0; i < _characters.Length; i++)
        {
            if (_characters[i] == null) continue;

            charaUIs[i].gameObject.SetActive(true);
            charaUIs[i].transform.Translate(count * charaUIDisSpace - harfWidth, 0, 0);  // ポジション設定
            charaUIs[i].Init(charaIDs[i], _characters[i].GetHP());
            _characters[i].OnDamage += ChangeHP;
            count++;
        }

        pauseUI.SetActive(false);

        // 一度更新
        UpdateMethod(_characters);
    }

    /// <summary>
    /// アップデート処理
    /// </summary>
    /// <param name="_characters"></param>
    public void UpdateMethod(CharacterController[] _characters)
    {
        var types = ProjectManager.Instance.GetPlayerTypes();
        for(int i = 0; i < _characters.Length; i++)
        {
            if(_characters[i] == null) continue;

            int numbersIndex = types[i] == PlayerType.CPU ? i + ProjectManager.MaxPlayer : i;
            // キャラが死んでいたら、行わない
            if (_characters[i].GetIsDead()) { playerNumbers[numbersIndex].SetActive(false); continue; }

            playerNumbers[numbersIndex].SetActive(true);

            Vector3 screenPos = Camera.main.WorldToScreenPoint(_characters[i].transform.position);

            playerNumbers[numbersIndex].transform.position = screenPos + (Vector3.up * playerNumberShiftPosY);
        }
    }

    /// <summary>
    /// ゲーム終了時の処理
    /// </summary>
    /// <param name="_characters"></param>
    public void GameEnd(CharacterController[] _characters)
    {
        for (int i = 0; i < _characters.Length; i++)
        {
            if(_characters[i] == null) continue;

            _characters[i].OnDamage -= ChangeHP;
        }
    }

    /// <summary>
    /// ポーズ処理
    /// </summary>
    /// <param name="flag"></param>
    public void Pause(bool flag)
    {
        pauseUI.SetActive(flag);

        // SE再生
        if (flag) SoundManager.Instance.SEPlay(SE.SHOW_WINDOW);
        else SoundManager.Instance.SEPlay(SE.BACK);

        if (!flag) return;

        eventSystem.SetSelectedGameObject(pauseUI_btn);
    }

    /// <summary>
    /// キャラUIのHPバーの値変更
    /// </summary>
    /// <param name="index">プレイヤー番号</param>
    /// <param name="value">現在HP</param>
    public void ChangeHP(int index, int value)
    {
        charaUIs[index].ChangeHPValue(value);
    }

    /// <summary>
    /// カウントダウンアニメーション
    /// </summary>
    /// <param name="count"></param>
    public async void CountDownText(int count)
    {
        if (countDownText == null) return;

        countDownText.gameObject.SetActive(true);

        for (int i = count; i > 0; i--)
        {
            countDownText.text = i.ToString();

            await Task.Delay(1000);
        }

        countDownText.text = "たたかえ!!";

        await Task.Delay(1000);

        countDownText.gameObject.SetActive(false);
    }
}
