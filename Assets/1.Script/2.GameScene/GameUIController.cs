using UnityEngine;
using UnityEngine.TextCore.Text;


/// <summary>
/// ゲーム中のUIを管理
/// </summary>
public class GameUIController : MonoBehaviour
{
    [SerializeField] CharaUI charaUIPrefab;
    [SerializeField] Transform charaUIParent;
    [SerializeField] float charaUIDisSpace;
    [SerializeField] GameObject pauseUI;

    [SerializeField] CharaVisualData[] charaVisualDatas;

    CharaUI[] charaUIs;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(CharacterController[] _characters)
    {
        // キャラアイコン 生成
        charaUIs = new CharaUI[_characters.Length];
        var harfWidth = (_characters.Length - 1) * charaUIDisSpace * 0.5f;
        for(int i = 0; i < _characters.Length; i++)
        {
            int charaID = 0;    // *** 仮 ***
            charaUIs[i] = Instantiate(charaUIPrefab, charaUIParent);
            charaUIs[i].transform.Translate(i * charaUIDisSpace - harfWidth, 0, 0);  // ポジション設定
            charaUIs[i].Init(charaVisualDatas[charaID].BustUp, _characters[i].GetHP()); // *** 仮 ***
            _characters[i].OnDamage += ChangeHP;
        }
        charaUIPrefab.gameObject.SetActive(false);

        pauseUI.SetActive(false);
    }

    public void GameEnd(CharacterController[] _characters)
    {
        for (int i = 0; i < _characters.Length; i++)
        {
            
            _characters[i].OnDamage -= ChangeHP;
        }
    }

    public void Pause(bool flag)
    {
        pauseUI.SetActive(flag);
    }

    public void ChangeHP(int index, int value)
    {
        charaUIs[index].ChangeHPValue(value);
    }
}
