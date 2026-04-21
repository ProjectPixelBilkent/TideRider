using UnityEngine;

public class SpeedBoostEffect : ExternalEffect
{
    [SerializeField] private string soundId = "speed_boost";
    private bool hasPlayedSound = false;

    public override Vector3 GetAddition(PlayerMovement ship)
    {
        if (!hasPlayedSound)
        {
            SoundLibrary.Instance.Play(soundId);
            hasPlayedSound = true;
        }

        return base.GetAddition(ship);
    }
}