using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private WeaponStat[] armory;
    public bool playerShip;

    private float[] lastFired;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(playerShip)
        {
            var playerArmory = DataManager.GetPlayerArmory();
            armory = new WeaponStat[playerArmory.Length];
            for (int i=0; i<armory.Length; i++)
            {
                armory[i] = new WeaponStat(playerArmory[i], 0);
            }
        }

        lastFired = new float[armory.Length];
        ResetTimes();
    }

    public void ResetTimes()
    {
        for(int i=0; i<lastFired.Length; i++)
        {
            lastFired[i] = Time.time;
        }
    }

    private void FixedUpdate()
    {
        for(int i=0; i< armory.Length ; i++)
        {
            if (armory[i].weaponInfo == null)
            {
                continue;
            }

            if(Time.time - lastFired[i] < armory[i].WeaponLevel.fireRate)
            {
                continue;
            }

            lastFired[i] = Time.time;

            //Might be better to implement an object pool.
            var currentBullet = Instantiate(armory[i].weaponInfo.projectilePrefab).GetComponent<Bullet>();

            currentBullet.Weapon = armory[i].weaponInfo;
            currentBullet.Level = armory[i].level;
            currentBullet.WeaponLevel = armory[i].WeaponLevel;

            currentBullet.transform.position = Weapon.BulletOffsets[i] + transform.position;
            currentBullet.Activate(Weapon.BulletDirections[i]);
        }
    }
}
