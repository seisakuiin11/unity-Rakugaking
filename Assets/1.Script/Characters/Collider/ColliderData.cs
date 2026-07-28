using System;
using UnityEngine;

[Serializable]
public struct CircleColData:Collider
{
    /// <summary>
    /// 円の半径
    /// </summary>
    public float radius;

    /// <summary>
    /// 追従するオブジェクトからの位置
    /// </summary>
    public Vector3 localPos;

    /// <summary>
    /// ゲーム内の位置
    /// </summary>
    public Vector3 worldPos { get {

            if (trans == null) Debug.Log("transないよ");

            var pos= trans.position + localPos;
            pos.x= trans.position.x + localPos.x* trans.localScale.x;
            return pos; } }




    public Vector3 LocalPos { get
        {
            var pos = localPos;
            pos.x = localPos.x * trans.localScale.x;
            return pos;
        }
    }


    public Vector3 WorldPos { get { return worldPos; } }


    /// <summary>
    /// 追従するTransform
    /// </summary>
    public Transform trans;


    public Transform Trans {  get { return trans; } }



    /// <summary>
    /// コライダーの特性(攻撃コライダーなど）)
    /// </summary>
    public COLLIDER_TYPE colType;

}


public interface Collider
{
    /// <summary>
    /// 追従するオブジェクトからの位置
    /// </summary>
   public Vector3 LocalPos { get;  }

    /// <summary>
    /// ゲーム内の位置
    /// </summary>
    public Vector3 WorldPos { get;}
    

    public Transform Trans { get; }
}


public enum COLLIDER_TYPE
{
    AttackCol,
    HitBox,
}