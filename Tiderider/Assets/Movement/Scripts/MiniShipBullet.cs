using UnityEngine;

public class MiniShipBullet : Bullet
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject enemy;

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
            direction = (enemy.transform.position - transform.position).normalized;
        }
        rigidBody.linearVelocity = (direction * WeaponLevel.speedOfBullet * 5f );
    }

    public override void Activate(Vector3 direction, Vector3 shipSpeed)
    {
        base.Activate(direction, shipSpeed);
        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        // for (int i = 0; i < enemyList.Length; i++)
        // {
        //     enemyList[i].transform.position
        // }
        enemy = enemyList[0];
        timer = 1000;


    }
    
}
