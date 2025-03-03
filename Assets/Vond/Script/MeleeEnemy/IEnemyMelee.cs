using UnityEngine;
using System.Collections;

public interface IEnemyMelee
{
    IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float damage, float attackInterval);
}
