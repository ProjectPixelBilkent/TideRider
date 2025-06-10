using UnityEngine;
using UnityEngine.UI;

// Bu satır, bu script'i eklediğin GameObject'te Image ve Weapon
// komponentlerinin bulunmasını zorunlu kılar. Bu, hata yapmanı önler.
[RequireComponent(typeof(Image), typeof(Weapon))]
public class WeaponButtonDisplay : MonoBehaviour
{
    // Awake, Start'tan bile önce çalışır. UI elemanlarını ayarlamak için idealdir.
    void Awake()
    {
        // 1. Bu GameObject üzerindeki diğer komponentleri al
        Weapon weaponData = GetComponent<Weapon>();
        Image buttonImage = GetComponent<Image>();

        // 2. Weapon verisindeki ikonu, Image komponentinin sprite'ına ata
        if (weaponData.weaponIcon != null)
        {
            // Eğer bir ikon atanmışsa, onu göster
            buttonImage.sprite = weaponData.weaponIcon;
        }
        else
        {
            // Eğer ikon atanmamışsa, alternatif olarak belirlediğin rengi göster
            // ve sprite'ı boşalt ki renk görünsün.
            buttonImage.sprite = null; 
            buttonImage.color = weaponData.weaponColor;
        }
    }
}