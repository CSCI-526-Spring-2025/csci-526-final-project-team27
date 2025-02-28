using UnityEngine;
using System.Collections;

/// <summary>
/// 攻击行为的接口
/// </summary>
public interface ITeammateMelee 
{
    IEnumerator Attack(MonoBehaviour owner, Transform self, Transform target, float damage, GameObject hitboxPrefab, float hitboxOffset, float attackCooldown);
}
