using UnityEngine;

public interface IAttackBehavior
{
    void Attack();
}


public class NormalAttack : MonoBehaviour, IAttackBehavior, IPlayerAttack
{
    public GameObject atkPointPrefab;
    public float attackDistance = 1f;
    public float attackDuration = 0.2f;
    public int damage = 10; // この攻撃のダメージ
    //public bool isRight = true;
    public bool isRight { get; set; }

    public void Attack()
    {
        Debug.Log(isRight);
        Vector3 direction = isRight ? transform.right : -transform.right;
        Vector3 atkPos = transform.position + direction * attackDistance;
        GameObject atkObj = Instantiate(atkPointPrefab, atkPos, Quaternion.identity);
        
        // ダメージ設定
        AttackHitBox hitBox = atkObj.GetComponent<AttackHitBox>();
        if (hitBox != null)
        {
            hitBox.SetDamage(damage);
        }
        
        Destroy(atkObj, attackDuration);
    }
}
