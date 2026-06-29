
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary>
/// すべてのコライダーを統括するクラス
/// </summary>

public class ColliderManager : MonoBehaviour
{
    List<AttackColliderList> attackColList = new();
    List<IHitBoxCol> hitBoxList = new();



    /// <summary>
    /// Update処理
    /// </summary>
    public void UpdateMethod()
    {

        if (attackColList.Count <= 0) return;

        foreach (var atk in attackColList)
        {
            atk.HitboxCheckProcess(hitBoxList);

        }
    }


    /// <summary>
    /// 攻撃コライダーの情報を更新（再生成）
    /// </summary>
    /// <param name="colData"></param>
    /// <param name="action"></param>
    /// <param name="atkColList"></param>
    /// <returns></returns>
    public AttackColliderList UpdateAttackColliderData(List<CircleColData> colData, Action<CharacterController,Vector3> action,AttackColliderList atkColList)
    {

        attackColList.Remove(atkColList);

        var newColList = new AttackColliderList(colData, action);


        attackColList.Add(newColList);

        return newColList;

    }


    
    /// <summary>
    /// 攻撃コライダーの情報を更新（生成）
    /// </summary>
    /// <param name="colData"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public AttackColliderList UpdateAttackColliderData(List<CircleColData> colData,Action<CharacterController,Vector3> action)
    {
        var newColList = new AttackColliderList(colData,action);


        attackColList.Add(newColList);

        return newColList;
    }


    public Collider_Circle AddNewHitBoxCircleCol(CircleColData colData, CharacterController chara)
    {
        var newCol = new HitBox_Circle(colData,chara);

        hitBoxList.Add(newCol);

        return newCol;
    }



    public void DestroyCircleCol(AttackColliderList col)
    {
       attackColList.Remove(col);
    }

    public void DestroyCircleCol(HitBox_Circle col)
    {
        hitBoxList.Remove(col);
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
    
        foreach (AttackColliderList colList in attackColList )
        {
            foreach (Collider_Circle col in colList.circleColList)
                Gizmos.DrawWireSphere(col.colData.worldPos, col.colData.radius);
        }

        Gizmos.color = Color.cyan;
        foreach (Collider_Circle col in hitBoxList)
        {
            Gizmos.DrawWireSphere(col.colData.worldPos, col.colData.radius);
        }

    }

}
