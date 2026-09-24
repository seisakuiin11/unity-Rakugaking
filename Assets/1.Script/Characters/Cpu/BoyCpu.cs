using UnityEngine;

/// <summary>
/// ナルボ CPU
/// </summary>
public class BoyCpu : CpuBase
{
    const float KenFuruAreaLength = 1.8f;
    const float KenNageruAreaLength = 8f;
    const int GenkiippaiMaxFlame = 15;
    bool GenkiippaiFlag;
    int GenkiippaiCount;

    BoyBehavior behavior;


    public override void Init(CharacterController _character)
    {
        base.Init(_character);
        behavior = _character.transform.GetComponent<BoyBehavior>();
    }

    /// <summary>
    /// アクションを考える 思考
    /// </summary>
    /// <param name="_characters"></param>
    public override InputCommandData Think(CharacterController[] _characters)
    {
        // ターゲットを決める
        SetTarget(_characters, out Vector3 myPos, out Vector3 targetPos);

        var data = new InputCommandData();
        float dis = Vector3.Distance(myPos, targetPos);
        float disX = Mathf.Abs(targetPos.x - myPos.x);
        float disY = targetPos.y - myPos.y;


        // 足場を降りる
        if (Descend(ref data)) return data;

        // ターゲットを向く
        LookTarget(ref data, myPos, targetPos);

        // 元気いっぱい中なら、移動しない
        if (GenkiippaiFlag)
        {
            data.DIRECTION_VEC2 = Vector2.zero;
            data.DIRECTION_DATA = DIRECTIONDATA.Neutral;
        }

        // 判定をするかしないかのフラグ
        clock = !clock;
        // 攻撃判定をしない時
        if (clock) return data;

        // 元気いっぱい発動中
        if (GenkiippaiFlag && GenkiippaiCount < GenkiippaiMaxFlame)
        {
            GenkiippaiCount++;
            data.ATTACK_UP = true;
            return data;
        }
        else if (GenkiippaiFlag) GenkiippaiFlag = false;

        // 剣を3本投げていたら
        if (behavior.GetSwordCount() >= 3 && CpuMoveRoll(0.3f))
        {
            data.ATTACK_LEFT = true;
            return data;
        }

        // 射程圏内で抽選の結果、剣を投げる
        if (Mathf.Abs(disY) <= VerticalDiff && disX < KenNageruAreaLength && CpuMoveRoll(0.2f))
        {
            data.ATTACK_RIGHT = true;
            return data;
        }
        else
        {
            // 剣を振るor元気いっぱい
            if (behavior.GetEnergyPercent() < 0.5f && CpuMoveRoll(0.3f))
            {
                GenkiippaiFlag = true;
                GenkiippaiCount = 0;
            }
            // 近くに敵がいるなら、剣を振る
            else if (disY >= -0.2 && dis < KenFuruAreaLength * behavior.SwordTrans.localScale.y)
            {
                data.ATTACK_DOWN = true;
                return data;
            }
        }


        //data.ATTACK_UP = true;

        // 敵が上にいたら、ジャンプ
        JumpThink(ref data, dis, disY);

        // 敵が真下にいたら、足場を降りる
        DescendThink(ref data, disY);

        return data;
    }
}
