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


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(CharacterController[] _characters)
    {
        // 一度非表示
        if (charaUIs != null)
            foreach (var cu in charaUIs) cu.gameObject.SetActive(false);
        if (playerNumbers != null)
            foreach (var pn in playerNumbers) pn.SetActive(false);

        // キャラアイコン 生成
        var harfWidth = (_characters.Length - 1) * charaUIDisSpace * 0.5f;
        for(int i = 0; i < _characters.Length; i++)
        {
            int charaID = 0;    // *** 仮 ***
            charaUIs[i].gameObject.SetActive(true);
            charaUIs[i].transform.Translate(i * charaUIDisSpace - harfWidth, 0, 0);  // ポジション設定
            charaUIs[i].Init(charaVisualDatas[charaID].BustUp, _characters[i].GetHP());
            _characters[i].OnDamage += ChangeHP;

            playerNumbers[i].SetActive(true);
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
        for(int i = 0; i < _characters.Length; i++)
        {
            // キャラが死んでいたら、行わない
            if (_characters[i].GetIsDead()) { playerNumbers[i].SetActive(false); continue; }

            playerNumbers[i].SetActive(true);

            Vector3 screenPos = Camera.main.WorldToScreenPoint(_characters[i].transform.position);

            playerNumbers[i].transform.position = screenPos + (Vector3.up * playerNumberShiftPosY);
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

        if (!flag) return;

        if (pauseUI_btn == null) return;

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
}
