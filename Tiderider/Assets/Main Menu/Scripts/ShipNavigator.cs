using UnityEngine;
using System;
using DG.Tweening;

public class ShipNavigator : MonoBehaviour
{
    [Header("DOTween Settings")]
    [SerializeField] private float speed = 400f;
    [SerializeField] private float rotationDuration = 0.3f;
    [SerializeField] private float edgePadding = 60f;
    [SerializeField] private Ease movementEase = Ease.InOutQuad;

    private Tween currentMoveTween;
    private Tween currentRotateTween;

    public void NavigateTo(GameObject targetLevelObj, Action arrivalCallback)
    {
        currentMoveTween?.Kill();
        currentRotateTween?.Kill();

        Vector3 targetPos = targetLevelObj.transform.position;

        float directionSign = Mathf.Sign(targetPos.x - transform.position.x);
        Vector3 offset = new Vector3(-directionSign * edgePadding, 0, 0);
        Vector3 finalDestination = targetPos + offset;

        float distance = Vector3.Distance(transform.position, finalDestination);
        float travelDuration = Mathf.Min(distance / speed, 2f);

        if (distance < 10f)
        {
            arrivalCallback?.Invoke();
            return;
        }

        Vector3 moveDir = (finalDestination - transform.position).normalized;
        float targetAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;

        currentRotateTween = transform.DORotateQuaternion(Quaternion.Euler(0, 0, targetAngle), rotationDuration)
            .SetEase(Ease.OutQuad);

        currentMoveTween = transform.DOMove(finalDestination, travelDuration)
            .SetEase(movementEase)
            .OnComplete(() => arrivalCallback?.Invoke());
    }

    public void SnapToLevel(GameObject targetLevelObj)
    {
        currentMoveTween?.Kill();
        currentRotateTween?.Kill();

        Vector3 targetPos = targetLevelObj.transform.position;

        float directionSign = -1f;
        Vector3 offset = new Vector3(-directionSign * edgePadding, 0, 0);

        transform.position = targetPos + offset;

        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}