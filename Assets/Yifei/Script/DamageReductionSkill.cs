using UnityEngine;

public class DamageReductionSkill : Skill
{
    protected override void OnSkillEffect(Vector2 direction)
    {
        return;
    }

    protected override void OnSkillEffect(GameObject target)
    {
        throw new System.NotImplementedException();
    }
}
