using UnityEngine;

public class CpuController : MonoBehaviour
{
    CharacterController[] characters;
    CpuBase[] cpu;


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(CharacterController[] _charcters)
    {
        characters = _charcters;
        cpu = new CpuBase[_charcters.Length];
        for(int i = 0; i < cpu.Length; i++)
        {
            cpu[i] = new CpuBase();
            cpu[i].Init(_charcters[i]);
        }
    }

    /// <summary>
    /// アップデート関数
    /// </summary>
    /// <param name="_characters">全キャラ</param>
    public void UpdateMethod(CharacterController[] _characters)
    {
        for(int i = 0; i < characters.Length; i++)
        {
            var data = cpu[i].Think(_characters);
            characters[i].SetCommandData(data);
        }
    }
}
