using UnityEngine;

public class PlayerAnimEvents : MonoBehaviour
{
    private ShootingController shootingController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get parents'gameobject 's shooting controller
        shootingController = GetComponentInParent<ShootingController>();
        if (shootingController!=null)
        {
            Debug.Log("GEEEEEEEEEEET");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnShootAnimEnd()
    {
        if(shootingController!=null)
        {
            shootingController.isOnCooldown = false;
        }
    }
}
