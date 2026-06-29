
using System;
using System.Collections.Generic;
using UnityEngine;



public class Collider_Circle
{
    
    /// <summary>
    /// ぶつかったプレイヤーのCharacterスクリプト
    /// </summary>
    public CharacterController targetCharaCon { get; private set; }




    /// <summary>
    /// Colliderの情報
    /// </summary>
    public  CircleColData colData { get; private set; }

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="_colData">コライダーの情報</param>
    public Collider_Circle(CircleColData _colData)
    {
        colData = _colData;

    }


   
    public void ReMove(Vector3 vec)
    {
        var newData = colData;
        newData.localPos = vec;
        colData = newData;
    }

    public void ReScale(float size)
    {
        var newData = colData;
        newData.radius = size;
        colData = newData;
    }

}


public class Attack_Circle : Collider_Circle,IAttackCol
{




    public Attack_Circle(CircleColData colData)
        : base(colData) { }


    public Vector3 WorldPos => colData.worldPos;

    /// <summary>
    /// コライダーがHitBoxにあたっているかを判断する
    /// </summary>
    /// <param name="hitBoxList"></param>
    public bool CircleHitBoxCheck(HitBox_Circle hitbox)
    {



            //対象のコライダーの持ち主が自分と一緒だった場合は除外
            if (colData.trans == hitbox.colData.trans) return false;

            

            //当たり判定
            var hitboxPos = hitbox.colData.worldPos;
            var addHalfRadius = colData.radius + hitbox.colData.radius;

            if (Vector3.Distance(colData.worldPos, hitboxPos) <= addHalfRadius)
            {
 

                //当たったヒットボックスをlistに保存
                return true;
                
            }

        return false;

    }
}



public class HitBox_Circle : Collider_Circle,IHitBoxCol
{
    public CharacterController controller { get; }
  
    public HitBox_Circle(CircleColData colData,CharacterController chara): base(colData) 
    {
    controller = chara;
    }

  
}

/// <summary>
/// 加害者コライダー
/// </summary>
public interface IAttackCol
{
    public bool CircleHitBoxCheck(HitBox_Circle hitBoxList);
}


/// <summary>
/// 被害者コライダー
/// </summary>
public interface IHitBoxCol
{
    public CharacterController controller { get; }
}

















/// <summary>
/// 攻撃のコライダーが格納されたリスト
/// </summary>
public  class AttackColliderList
{
    /// <summary>
    /// ヒットしたコライダーリスト
    /// </summary>
    public List<IHitBoxCol> isHitBoxList { get; set; } = new();

    /// <summary>
    ///　サークルコライダーリスト
    /// </summary>
    public List<Attack_Circle> circleColList = new();



    public Action<CharacterController,Vector3> action { get; set; }


    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="_colList"></param>
    public AttackColliderList(List<CircleColData> _colList, Action<CharacterController,Vector3> _action)
    {
        action += _action;
       foreach (var col in _colList)
        {
            circleColList.Add(new Attack_Circle(col));
        }
    }



   




    /// <summary>
    /// コライダーが当たっているか判定する
    /// </summary>
    /// <param name="hitBoxList"></param>
    public void HitboxCheckProcess(List<IHitBoxCol> hitBoxList)
    {
        foreach (HitBox_Circle hitbox in hitBoxList)
        {
            //対象のコライダーがすでにこのコライダー群にhitしていたら無視
            if (isHitBoxList.Contains(hitbox)) continue;


            foreach (var col in circleColList)
            {
                //あたっていない場合は無視
                if (!col.CircleHitBoxCheck(hitbox)) continue;


                action.Invoke(hitbox.controller, col.WorldPos);
                isHitBoxList.Add(hitbox);
                
            }

        }



    }


}