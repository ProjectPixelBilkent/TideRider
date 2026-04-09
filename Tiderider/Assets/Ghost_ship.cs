//using System;
//using UnityEngine;

//public class Ghost_ship : MonoBehaviour
//{
//    public float Health;
//    public float MaxHealth;
//    public int status; 
//    public SpriteRenderer Ship_Sprite;
//    public Transform Player_Position;
//    public Rigidbody2D rb;
//    public float Ghost_Ship_Speed;
//    public int Current_Ammo;
//    public int MaxAmmo;
//    public Collider2D GhostShipCollider;
//    public float CanonCoolDown;
//    public float CanonFireTimer;
//    void Start()
//    {
//        Health = MaxHealth;
//        status = 0;
//        MaxAmmo = 1;
//        Current_Ammo = MaxAmmo;
//        CanonFireTimer = 0;
        
//    }

//    // Update is called once per frame
//    void Update()
//    {

//        //status 1 is sailing
//        // status 2 is diving(dissapearing)
//        //status 3 is attacking
       

//        if(Current_Ammo > 0)
//        {
//            status = 3;
//        }else if (Current_Ammo <= 0)
//        {
//            status = 2;
//        }
//        if (status == 0)
//        {

//        }
//        else if (status == 1)
//        {
//            Sailing(Ghost_Ship_Speed);
//        }
//        else if (status == 2)
//        {
//            Diving();
//        }
//        if(status == 3)
//        {
//            Attack();
//        }
        
//    }
//    public void FixedUpdate()
//    {
        
//    }
//    public void Diving()
//    {
//        Ship_Sprite.enabled = false;
//        GhostShipCollider.enabled = false;
//    }
//    public void Attack()
//    {
//        if(Current_Ammo > 0 && CanonFireTimer <= 0)
//        {
//            Current_Ammo -= 1;
//            CanonFireTimer = CanonCoolDown;

//        }else if (Current_Ammo > 0 && CanonFireTimer > 0)
//        {
//            CanonFireTimer -= Time.deltaTime;
//        }
//        if(Current_Ammo <= 0)
//        {
//            status = 2;
//        }

               
        
//    }
//    public void Sailing(float speed)
//    {

//    }
    
//}
