
using UnityEngine;
using System.Collections;

public class EnemyMeleeAttacker : IEnemyMelee
{
    public IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float damage, float attackInterval)
    {
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning("目标缺少 Health 组件，无法造成伤害");
        }
        yield return new WaitForSeconds(attackInterval);
    }
}
