using UnityEngine;

public class WeaponAbstract : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int[] damage;
    [SerializeField] private int level;
}
