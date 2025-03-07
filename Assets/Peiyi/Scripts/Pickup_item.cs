using UnityEngine;

public class Pickup_item : MonoBehaviour
{
    public Item item; 

    void OnTriggerEnter2D(Collider2D other) 
{

    if (other.CompareTag("Player"))
    {   
        Debug.Log("item hit"); 
        BagManager inventory = FindObjectOfType<BagManager>();
        inventory.AddItem(item);
        Debug.Log("Destroying: " + gameObject.name);
        Destroy(gameObject); 
    }
}
}
