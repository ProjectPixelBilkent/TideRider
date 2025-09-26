using System;
using UnityEngine;

[Serializable]
public struct WeaponStat
{
    public Weapon weaponInfo;
    public int weaponLevel;
}

public class CommonPirate : MonoBehaviour
{
    [SerializeField] private WeaponStat[] pirateArmory;
    
    private ShipModel pirateModel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pirateModel = GetComponent<ShipModel>();
        pirateModel.HealthChanged += OnHealthChanged;
    }

    public void OnHealthChanged()
    {
        if(pirateModel.CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
