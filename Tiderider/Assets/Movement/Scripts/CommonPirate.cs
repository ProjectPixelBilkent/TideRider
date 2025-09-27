using System;
using UnityEngine;



public class CommonPirate : MonoBehaviour
{
    public ShipModel pirateModel;

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
}
