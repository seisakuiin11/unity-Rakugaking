
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


/// <summary>
/// 攻撃コライダー（円）
/// </summary>
public class Attack_Circle : Collider_Circle,IAttackCol
{



    public Attack_Circle(CircleColData colData)
        : base(colData) {  }


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

public class PushBox_Circle : Collider_Circle
{
    public CharacterController controller { get; }

    public float pastX { get; private set; }

    const float pushForce =0.1f;



    public PushBox_Circle(CircleColData colData, CharacterController chara) : base(colData)
    {
        controller = chara;
        pastX = colData.worldPos.x;
    }

    public void Push(float xPower)
    {
        controller.Move(new Vector2(xPower, 0));
    }

    public void HitCheck(PushBox_Circle targetCol)
    {

  
        //当たり判定      
        var targetPos = targetCol.colData.worldPos;
        var addHalfRadius = colData.radius + targetCol.colData.radius;

        if (Vector3.Distance(colData.worldPos, targetPos) > addHalfRadius) 
        {

            targetCol = null;
            return; 
        }

        //左右どちらにずれるかの判定
        float i = colData.worldPos.x - targetPos.x;

        PushJudge(i,targetCol);

        targetCol = null;

    }

    /// <summary>
    /// 左右どちらにずれるかの判定
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetCol"></param>
    private void PushJudge(float value, PushBox_Circle targetCol)
    {
        //重なっていたら過去の位置を参照
        if (value == 0)
        {
            value = pastX - targetCol.pastX;
            
            //それでも重なっていたら画面の左右で分ける
            if (value == 0)
            {


                //y軸を参照して上のほうが優先して反対側に行く
                float targetY = targetCol.colData.worldPos.y;
                float i = colData.worldPos.y - targetY;


                if (i > 0)
                {
                    //自身を優先する
                    value = colData.worldPos.x;
                }
                else
                {
                    //相手を優先する
                    value = -(targetCol.colData.worldPos.x);
                }

            }
               
        }

        float dir = Mathf.Sign(value);

        Push(dir * pushForce);
        targetCol.Push(-dir * pushForce);

    }

    /// <summary>
    /// pastXを保存する
    /// </summary>
    public void SavePastX()
    {
        pastX = colData.worldPos.x;
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





public class HitBoxColliderList
{
        /// <summary>
    ///　サークルコライダーリスト
    /// </summary>
    public List<HitBox_Circle> circleColList = new();

  /// <summary>
  /// 初期化
  /// </summary>
  /// <param name="_colList"></param>
  /// <param name="chara">Hitboxの持ち主</param>
    public HitBoxColliderList(List<CircleColData> _colList,CharacterController chara)
    {
        foreach (var col in _colList)
        {
            circleColList.Add(new HitBox_Circle(col,chara));
        }
    }

    public IEnumerator<HitBox_Circle> GetEnumerator()
    {
        return circleColList.GetEnumerator();
    }
}











/// <summary>
/// 攻撃のコライダーが格納されたリスト
/// </summary>
public  class AttackColliderList
{
    /// <summary>
    /// ヒットしたコライダーリスト
    /// </summary>
    public List<HitBoxColliderList> isHitBoxList = new();

    /// <summary>
    ///　サークルコライダーリスト
    /// </summary>
    public List<Attack_Circle> circleColList = new();



    public Action<CharacterController,Collider> action { get; set; }


    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="_colList"></param>
    public AttackColliderList(List<CircleColData> _colList, Action<CharacterController,Collider> _action)
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
    public bool HitboxCheckProcess(HitBoxColliderList hitBoxList)
    {
        foreach (var col in circleColList)
        {
            

                //対象のコライダーがすでにこのコライダー群にhitしていたら無視
                if (isHitBoxList.Contains(hitBoxList)) continue;

                foreach (HitBox_Circle hitBox in hitBoxList)
                {
                    //あたっていない場合は無視
                    if (!col.CircleHitBoxCheck(hitBox)) continue;



                action.Invoke(hitBox.controller, col.colData);
                    isHitBoxList.Add(hitBoxList);
                   return true;
                }
                
            

        }
        return false;


    }


}