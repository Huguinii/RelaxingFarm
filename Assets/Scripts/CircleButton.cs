using UnityEngine;
using UnityEngine.UI;

public class CircleButton : Image
{
    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screenPoint, eventCamera, out Vector2 localPoint);

        // Calcula si el punto está dentro del círculo
        Vector2 normalizado = new Vector2(
            localPoint.x / rectTransform.rect.width,
            localPoint.y / rectTransform.rect.height);

        return normalizado.magnitude <= 0.5f;
    }
}