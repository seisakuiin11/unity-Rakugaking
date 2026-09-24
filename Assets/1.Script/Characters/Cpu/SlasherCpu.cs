using UnityEngine;

/// <summary>
/// スラッシャー CPU
/// </summary>
public class SlasherCpu : CpuBase
{
    const float SomersaultAreaLength = 2.0f;
    const float KeriageAreaLength = 2.2f;
    const float AccelAreaLength = 4.0f;
    const float AccelVerticalDiff = 2.0f;
    const float DiveSobatAreaLength = 7f;

    bool accelFlag;

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

        if (targetPlayer.State() == CHARA_STATE.atkUp)
        {
            data.ATTACK_DOWN = true;
            return data;
        }

        if (targetPlayer.State() == CHARA_STATE.atkRight && disX < 1f)
        {
            data.SHIELD = true;
            return data;
        }

        if(targetPlayer.State() == CHARA_STATE.atkUp)
        {
            data.DIRECTION_VEC2 += Vector2.up;
            data.JUMP = true;
        }

        // 相手が下にいて、自分が空中にいるなら、一定距離以内なら、地ならしを発動
        if (myCharacter.State() == CHARA_STATE.attack && dis < KeriageAreaLength)
        {
            data.ATTACK_DOWN = true;
            return data;
        }

        // 一定距離以内で、
        if (disY > 0 && dis < SomersaultAreaLength)
        {
            data.ATTACK_LEFT = true;
            return data;
        }

        // ある程度距離を詰められたら、アクセルで連打
        if (Mathf.Abs(disY) <= AccelVerticalDiff && dis < AccelAreaLength)
        {
            // 敵が前方にいる確認
            var foward = myCharacter.transform.lossyScale.x;
            var dif = (targetPos.x - myPos.x) / disX;
            if(foward != dif) return data;

            data.ATTACK_UP_OLD = accelFlag;
            accelFlag = !accelFlag;
            data.ATTACK_UP = accelFlag;
            return data;
        }
        else accelFlag = false;

        // 敵が真横にいて、ある程度の距離にいる場合、
        if (Mathf.Abs(disY) <= VerticalDiff && disX < DiveSobatAreaLength)
        {
            if (targetPlayer.State() == CHARA_STATE.atkDown && myCharacter.State() != CHARA_STATE.air)
            {
                data.DIRECTION_VEC2 += Vector2.up;
                data.JUMP = true;
                return data;
            }

            // 60%の確率
            if (CpuMoveRoll(0.6f)&&targetPlayer.State() != CHARA_STATE.atkRight)
            {
                data.ATTACK_RIGHT = true;
                return data;
            }
        }

        // 敵が上にいたら、ジャンプ
        JumpThink(ref data, dis, disY);

        // 敵が真下にいたら、足場を降りる
        DescendThink(ref data, disY);

        return data;
    }
}
