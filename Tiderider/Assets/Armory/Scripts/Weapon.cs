using UnityEngine;
using UnityEngine.UI; // Belki ileride UI elemanları eklersin diye

// [System.Serializable] etiketini kaldırıyoruz.
// Artık bu bir MonoBehaviour, yani bir Unity komponenti.
public class Weapon : MonoBehaviour
{
    public string weaponName;
    public Color weaponColor = Color.white;
    public Sprite weaponIcon;

    public string weaponDescription;
    
    public int weaponDamage;


}