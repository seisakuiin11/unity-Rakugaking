using UnityEngine;

public class AnimationTester : MonoBehaviour
{
    [SerializeField] Animator anim;

    bool air;

    private void OnEnable()
    {
        anim.SetTrigger("Idle");
    }

    // Update is called once per frame
    void Update()
    {
        // ジャンプ 陸空切り替え
        if (Input.GetKeyDown(KeyCode.Space)) { anim.SetBool("Air", air ^= true); anim.SetTrigger("Jump"); }

        // 左に移動
        if (Input.GetKeyDown(KeyCode.A)) { transform.localScale = new Vector3(-1, 1, 1); anim.SetTrigger("Move"); }
        if (Input.GetKeyUp(KeyCode.A)) anim.SetTrigger("Idle");

        // 右に移動
        if (Input.GetKeyDown(KeyCode.D)) { transform.localScale = new Vector3(1, 1, 1); anim.SetTrigger("Move"); }
        if (Input.GetKeyUp(KeyCode.D)) anim.SetTrigger("Idle");

        if (Input.GetKeyDown(KeyCode.UpArrow)) anim.SetTrigger("AttackUp");
        if (Input.GetKeyDown(KeyCode.DownArrow)) anim.SetTrigger("AttackDown");
        if (Input.GetKeyDown(KeyCode.LeftArrow)) anim.SetTrigger("AttackLeft");
        if (Input.GetKeyDown(KeyCode.RightArrow)) anim.SetTrigger("AttackRight");
    }
}