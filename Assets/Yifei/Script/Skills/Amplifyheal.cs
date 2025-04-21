using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Amplifyheal : Skill
{
    [Header("Heal Amounts")]
    [Tooltip("Default heal per click (flat)")]
    public float defaultHealAmount = 10f;
    [Tooltip("Amplified heal per click while skill is active")]
    public float amplifiedHealAmount = 15f;

    // flag to know which heal value to use
    private bool isAmplified = false;
    private GameObject player;
    private ShootingController shootingController;

    /// <summary>
    /// Called by the base Skill class when you activate this skill (e.g. press the numkey).
    /// </summary>
    void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogError("Amplifyheal: Player reference missing!");
        if (player != null)
            shootingController = player.GetComponent<ShootingController>();
        else
            Debug.LogError("Amplifyheal: ShootingController reference missing!");
    }
    protected override void OnSkillEffect(Vector2 direction)
    {
        StartAmplify();
    }

    protected override void OnSkillEffect(GameObject target)
    {
        StartAmplify();
    }

    private void StartAmplify()
    {
        isAmplified = true;
        shootingController.bulletPrefab.GetComponent<HealBallVond>().healAmount = amplifiedHealAmount;
        //Debug.Log($"[AmplifyHeal] Now healing for {amplifiedHealAmount} HP per click!");
        StartCoroutine(SkillRoutine());
    }

    /// <summary>
    /// Call this from your click‑to‑heal logic instead of the hard‑coded 10.
    /// </summary>
    public void HealTarget(GameObject ally)
    {
        var health = ally.GetComponent<Health_BC>();
        if (health == null) return;

        float healAmt = isAmplified ? amplifiedHealAmount : defaultHealAmount;
        health.Heal(healAmt);
        //Debug.Log($"[AmplifyHeal] Healed {ally.name} for {healAmt} HP");
    }

    /// <summary>
    /// After skillDuration seconds, turn off amplification.
    /// </summary>
    protected override IEnumerator SkillRoutine()
    {
        yield return new WaitForSeconds(skillDuration);

        isAmplified = false;
        shootingController.bulletPrefab.GetComponent<HealBallVond>().healAmount = defaultHealAmount;
        //Debug.Log($"[AmplifyHeal] Duration ended — healing back to {defaultHealAmount} HP");
    }
}
