using System;
using UnityEngine;


/// <summary>
/// stringデータから正規のデータに変換するクラス
/// </summary>
public class CharaDataAdapter : MonoBehaviour
{

    private void Start()
    {
      AttackState state=  StringToAttackClass("aa");

        state.StateStart();
    }

    public AttackState StringToAttackClass(string _data)
    {
        Type type = Type.GetType("NarboLeftAttackState");
        if (type == null) Debug.Log("存在しません");
        else Debug.Log("存在します");

        AttackState state=(AttackState)Activator.CreateInstance(type);
        var i = Activator.CreateInstance(type);
        return state;
    }
}
