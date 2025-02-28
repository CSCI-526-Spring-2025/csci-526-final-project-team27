using UnityEngine;

public class SlimeHealth : EnemyHealth
{
    public GameObject slimePrefab;  // Prefab for spawning new slimes
    public float splitThreshold = 0.5f; // Slime splits when health is below 50%
    public int splitCounter = 8; // Starts at 8 and halves after each split

    private bool hasSplit = false; // Prevent multiple splits at the same threshold

    void Update()
    {
        if (!hasSplit && splitCounter > 1 && currentHealth <= maxHealth * splitThreshold)
        {
            Split();
            hasSplit = true; // Prevent further splits at this threshold
        }
    }

    private void Split()
    {
        Debug.Log(gameObject.name + " is splitting!");

        for (int i = 0; i < 2; i++)
        {
            // Spawn new slime at a slight offset from the parent position
            GameObject newSlime = Instantiate(slimePrefab, transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f)), Quaternion.identity);

            // Get the health component of the new slime
            SlimeHealth newSlimeHealth = newSlime.GetComponent<SlimeHealth>();

            if (newSlimeHealth != null)
            {
                newSlimeHealth.maxHealth = maxHealth / 2; // New slime has half of parent's health
                newSlimeHealth.currentHealth = maxHealth / 2;
                newSlimeHealth.splitCounter = splitCounter / 2; // New slime gets half the split counter
            }
        }

        // Optionally destroy the parent slime after splitting
        Destroy(gameObject);
    }
}
