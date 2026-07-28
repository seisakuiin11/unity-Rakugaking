
using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// すべてのコライダーを統括するクラス
/// </summary>

public class ColliderManager : MonoBehaviour
{
    List<AttackColliderList> attackColList = new();     //攻撃コライダー群
    List<HitBoxColliderList> hitBoxList = new();        //当たり判定コライダー群
    List <PushBox_Circle> pushBoxList = new();  　　　　//押し出しコライダー群


    /// <summary>
    /// Update処理
    /// </summary>
    public void UpdateMethod()
    {
       AttackColliderCheck();
    
       PushBoxUpdate();
    }

    /// <summary>
    /// 攻撃コライダーが当たり判定コライダーにあたっているか確認する
    /// </summary>
    private void AttackColliderCheck()
    {

        bool hitFlag = false;

        if (attackColList.Count <= 0) return;


        foreach (var atk in attackColList)
        {
            foreach (var hitBox in hitBoxList)
            {
                hitFlag = atk.HitboxCheckProcess(hitBox);
                if (hitFlag) break;
            }

            if (hitFlag) break;

        }

        //衝突していたらもう一度処理を呼び出す（他の攻撃コライダーが当たっているか確認するため）
        if (hitFlag) AttackColliderCheck();
    }

    /// <summary>
    /// プッシュボックスのupdate
    /// </summary>
    private void PushBoxUpdate()
    {


        // プッシュボックス同士が当たっているか確認する

        //押し出し判定の個数が１つ以下なら何もしない
        if (pushBoxList.Count <= 1) return;

        for(int i = 0; i < pushBoxList.Count; i++)
        {
            for(int j = i+1; j < pushBoxList.Count; j++)
            {
                pushBoxList[i].HitCheck(pushBoxList[j]);                
            }
        }


        //過去座標の保存
        for (int i = 0; i < pushBoxList.Count; i++)
        {
            pushBoxList[i].SavePastX();
        }
    }


    /// <summary>
    /// 攻撃コライダーの情報を更新（再生成）
    /// </summary>
    /// <param name="colData"></param>
    /// <param name="action"></param>
    /// <param name="atkColList"></param>
    /// <returns></returns>
    public AttackColliderList UpdateAttackColliderData(List<CircleColData> colData, Action<CharacterController,Collider> action,AttackColliderList atkColList)
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
    public AttackColliderList UpdateAttackColliderData(List<CircleColData> colData,Action<CharacterController,Collider> action)
    {
        var newColList = new AttackColliderList(colData,action);


        attackColList.Add(newColList);

        return newColList;
    }

    /// <summary>
    /// 当たり判定コライダーの情報を更新（生成）
    /// </summary>
    /// <param name="colData"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public HitBoxColliderList UpdateHitBoxColliderData(List<CircleColData> colData, CharacterController chara)
    {
        var newColList = new HitBoxColliderList(colData,chara);

        hitBoxList.Add(newColList);

        return newColList;
    }

    /// <summary>
    /// 押し出しコライダーの情報を更新
    /// </summary>
    /// <param name="colData"></param>
    /// <param name="chara"></param>
    /// <returns></returns>
    public PushBox_Circle UpdatePushBoxColliderData(CircleColData colData,CharacterController chara)
    {
        var newCol = new PushBox_Circle(colData, chara);
        
        pushBoxList.Add(newCol);

        return newCol;
    }


    public void DestroyCircleCol(AttackColliderList col)
    {
       attackColList.Remove(col);
    }

    public void DestroyCircleCol(HitBoxColliderList col)
    {
        hitBoxList.Remove(col);
    }

    public void DestroyCircleCol(PushBox_Circle col)
    {
        pushBoxList.Remove(col);
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
    
        foreach (AttackColliderList colList in attackColList )
        {
            foreach (Collider_Circle col in colList.circleColList)
                Gizmos.DrawWireSphere(col.colData.worldPos, col.colData.radius);
        }

        Gizmos.color = Color.yellow;
       ;

        foreach (AttackColliderList colList in attackColList)
        {
            foreach (Collider_Circle col in colList.circleColList)
            {
                var vec = new Vector3(col.colData.LocalPos.x, col.colData.LocalPos.y, col.colData.LocalPos.z);
                Gizmos.DrawWireSphere(col.colData.worldPos - vec, col.colData.radius + 2);
            } 
        }


        Gizmos.color = Color.cyan;
        foreach (HitBoxColliderList colList in hitBoxList)
        {
            foreach (Collider_Circle col in colList.circleColList)
                Gizmos.DrawWireSphere(col.colData.worldPos, col.colData.radius);
        }

        Gizmos.color = Color.green;

        foreach (PushBox_Circle col in pushBoxList)
        {
            Gizmos.DrawWireSphere(col.colData.worldPos, col.colData.radius);
        }
    }

}
