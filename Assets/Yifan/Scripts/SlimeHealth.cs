using UnityEngine;
using UnityEngine.U2D;

public class SlimeHealth : EnemyHealth
{
    public GameObject slimePrefab;  // Prefab for spawning new slimes
    public float splitThreshold = 0.5f; // Slime splits when health is below 50%
    public int splitCounter = 4; // Starts at 8 and halves after each split
    public float jumpCounter = 1.0f;

    private bool hasSplit = false; // Prevent multiple splits at the same threshold

    const float fac = 0.06f;

    private MeleeEnemy slimeEnemy;
    private ParabolicSlimeJumpMovementBehavior2D jumpAction;

    void Start()
    {
        slimeEnemy = slimePrefab.GetComponent<MeleeEnemy>();
        jumpAction = slimePrefab.GetComponent<ParabolicSlimeJumpMovementBehavior2D>();
    }

    void Update()
    {
        if (!hasSplit && splitCounter > 1 && currentHealth <= maxHealth * splitThreshold)
        {
            Split();
            hasSplit = true; // Prevent further splits at this threshold
        }

        if (currentHealth <= 0) DeadSlime();

        // if (slimeEnemy.currentTarget != null && jumpAction != null) jumpAction.Move(slimeEnemy.transform, slimeEnemy.GetComponent<Rigidbody2D>(), slimeEnemy.currentTarget, slimeEnemy.moveSpeed);
    }

    void FixedUpdate()
    {
        if (slimeEnemy.currentTarget != null && jumpAction != null) jumpAction.Move(slimeEnemy.transform, slimeEnemy.GetComponent<Rigidbody2D>(), slimeEnemy.currentTarget, slimeEnemy.moveSpeed);
    }

    private void Split()
    {
        Debug.Log(gameObject.name + " is splitting!");
        GameObject[] newSlimes = new GameObject[2];

        for (int i = 0; i < 2; i++)
        {
            // Spawn new slime at a slight offset from the parent position
            GameObject newSlime = Instantiate(slimePrefab, transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f)), Quaternion.identity);
            newSlimes[i] = newSlime;
            float sSize = fac * (float)splitCounter + Random.Range(0, 0.09f);
            newSlime.transform.localScale = new Vector3(sSize, sSize, sSize);
            SpriteShapeRenderer sRenderer = newSlime.GetComponent<SpriteShapeRenderer>();
            switch (splitCounter)
            {
                case 4:
                    sRenderer.color = Color.blue;
                    break;
                case 2:
                    sRenderer.color = Color.cyan;
                    break;
            }

            // Get the health component of the new slime
            SlimeHealth newSlimeHealth = newSlime.GetComponent<SlimeHealth>();
            MeleeEnemy newSlimeDamage = newSlime.GetComponent<MeleeEnemy>();

            if (newSlimeHealth != null)
            {
                newSlimeHealth.maxHealth = currentHealth / 2; // New slime has half of parent's health
                newSlimeHealth.splitCounter = splitCounter / 2; // New slime gets half the split counter
                newSlimeHealth.jumpCounter = jumpCounter * 2.0f;
            }
            if (newSlimeDamage != null)
            {
                int damage = (int)(newSlimeDamage.damage * 0.5f);
                newSlimeDamage.damage = (float)damage;
                newSlimeDamage.moveSpeed = 3.0f * sSize + Random.Range(0, 0.09f);
            }
        }
        OnIncrease.Invoke(gameObject, newSlimes);
        OnDeath.Invoke(gameObject);
        // Optionally destroy the parent slime after splitting
        Destroy(gameObject);
    }

    private void DeadSlime()
    {
        Debug.Log(this.gameObject.name + " is dead");
        OnDeath.Invoke(gameObject);
        Destroy(gameObject);
    }
}
