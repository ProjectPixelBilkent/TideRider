using UnityEngine;

public class MiniShipBullet : Bullet
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Enemy { get; set; }


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private float timer = 0;
    private static float updatePeriod = 0.5f;
    protected override void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer >= updatePeriod)
        {
            timer = 0;
            if(Enemy!=null)
            {
                direction = (Enemy.transform.position - transform.position + Random.onUnitSphere * 1.5f).normalized;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; 
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        rigidBody.linearVelocity = Enemy==null ? Vector3.zero: (direction * WeaponLevel.speedOfBullet);
    }

    public override void Activate(Vector3 direction, Vector3 shipSpeed)
    {
        base.Activate(direction, shipSpeed);
        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        // for (int i = 0; i < enemyList.Length; i++)
        // {
        //     enemyList[i].transform.position
        // }

        if(enemyList.Length > 0)
        {
            Enemy = enemyList[0];
        }
        timer = 1000;


    }
    
    protected override void OnCollisionEnter2D(Collision2D collision) { }
    
}
