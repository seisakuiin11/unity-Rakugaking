using System;
using System.Collections.Generic;
using UnityEngine;

public class CpuController : MonoBehaviour
{
    CharacterController[] characters;
    CpuBase[] cpu;
    Dictionary<CharaDataManager.CharaID, Func<CpuBase>> cpuPrefab = new()
    {
        {CharaDataManager.CharaID.NARBO, () => new NarboCpu() },
        {CharaDataManager.CharaID.SLASHER, () => new SlasherCpu() },
        {CharaDataManager.CharaID.BOY, () => new BoyCpu() },
        {CharaDataManager.CharaID.ZERA, () => new ZeraCpu() },
    };


    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(CharacterController[] _charcters)
    {
        characters = _charcters;
        var charaIDs = GetCharaIDs();
        cpu = new CpuBase[_charcters.Length];

        for (int i = 0; i < cpu.Length; i++)
        {
            CpuBase _cpu;
            var id = (CharaDataManager.CharaID)charaIDs[i];
            Debug.Log(id);
            // CPUが存在するか確認する
            if (cpuPrefab.ContainsKey(id)) _cpu = cpuPrefab[id]();
            else _cpu = new CpuBase();

            cpu[i] = _cpu;
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

    // キャラIDの配列データから、CPUのキャラIDデータだけを抽出する
    int[] GetCharaIDs()
    {
        List<int> ids = new List<int>();
        var charaIDs = ProjectManager.Instance.GetCharaIDs();
        var playerTypes = ProjectManager.Instance.GetPlayerTypes();

        for (int i = 0; i < playerTypes.Length; i++)
        {
            if (playerTypes[i] != PlayerType.CPU) continue;

            ids.Add(charaIDs[i]);
        }

        return ids.ToArray();
    }
}
