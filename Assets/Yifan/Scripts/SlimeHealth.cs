using UnityEngine;
using UnityEngine.U2D;

public class SlimeHealth : EnemyHealth
{
    public GameObject slimePrefab;  // Prefab for spawning new slimes
    public float splitThreshold = 0.5f; // Slime splits when health is below 50%
    public int splitCounter = 8; // Starts at 8 and halves after each split

    private bool hasSplit = false; // Prevent multiple splits at the same threshold

    const float fac = 0.03f;

    void Update()
    {
        if (!hasSplit && splitCounter > 1 && currentHealth <= maxHealth * splitThreshold)
        {
            Split();
            hasSplit = true; // Prevent further splits at this threshold
        }

        if (currentHealth <= 0) DeadSlime();
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
            float sSize = fac * (float)splitCounter;
            newSlime.transform.localScale = new Vector3(sSize, sSize, sSize);
            SpriteShapeRenderer sRenderer = newSlime.GetComponent<SpriteShapeRenderer>();
            switch (splitCounter)
            {
                case 8:
                    sRenderer.color = Color.blue;
                    break;
                case 4:
                    sRenderer.color = Color.cyan;
                    break;
                case 2:
                    sRenderer.color = Color.gray;
                    break;
            }

            // Get the health component of the new slime
            SlimeHealth newSlimeHealth = newSlime.GetComponent<SlimeHealth>();

            if (newSlimeHealth != null)
            {
                newSlimeHealth.maxHealth = maxHealth / 2; // New slime has half of parent's health
                newSlimeHealth.currentHealth = maxHealth / 2;
                newSlimeHealth.splitCounter = splitCounter / 2; // New slime gets half the split counter
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
