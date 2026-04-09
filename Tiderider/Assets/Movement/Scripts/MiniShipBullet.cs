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
        transform.position += 0.5f * Time.deltaTime * LevelController.UpwardsMovement;
    }

    private float timer = 0;
    private static readonly float updatePeriod = 0.5f;

    protected override void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer >= updatePeriod)
        {
            timer = 0;
            if(Enemy!=null)
            {
                var ab = Enemy.transform.position - transform.position;
                direction = (ab + 1.3f * Random.onUnitSphere / (ab.magnitude + 1)).normalized;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; 
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        rigidBody.linearVelocity = Enemy==null ? Vector3.zero: (direction * WeaponLevel.speedOfBullet);
    }

    public override void Activate(Vector3 direction, Vector3 shipSpeed)
    {
        base.Activate(direction, shipSpeed);
        var enemyList = GameObject.FindGameObjectsWithTag(PlayerBullet ? "Enemy": "Player");

        Enemy = null;
        float closestDistance = float.MaxValue;

        foreach(var e in enemyList)
        {
            float dist = (e.transform.position - transform.position).magnitude;
            if(dist < closestDistance)
            {
                closestDistance = dist;
                Enemy = e;
            }
        }

        timer = 1000;
    }
    
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
    }
    
}
