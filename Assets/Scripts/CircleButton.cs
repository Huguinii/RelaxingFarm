// Extiende el componente Image de Unity para que el área de interacción del botón sea circular en lugar de rectangular.
// Útil para botones redondos (como el de la regadera) donde los clics en las esquinas no deben registrarse.

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente Image con detección de clic circular.
/// Se usa como componente gráfico en botones con forma de círculo para que solo los clics dentro del radio sean válidos.
/// </summary>
public class CircleButton : Image
{
    /// <summary>
    /// Sobreescribe la validación de posición de raycast para restringirla a un área circular en lugar del rectángulo completo del componente.
    /// </summary>
    /// <param name="screenPoint">Posición del puntero en coordenadas de pantalla.</param>
    /// <param name="eventCamera">Cámara que renderiza el canvas (null en overlay).</param>
    /// <returns>True si el punto está dentro del círculo inscrito en el RectTransform.</returns>
    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // Convertir el punto de pantalla a coordenadas locales del RectTransform
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screenPoint, eventCamera, out Vector2 localPoint);

        // Normalizar el punto respecto al tamaño del rect para comparar con el radio 0.5
        Vector2 normalizado = new Vector2(
            localPoint.x / rectTransform.rect.width,
            localPoint.y / rectTransform.rect.height);

        // El punto es válido si está dentro del círculo (radio normalizado = 0.5)
        return normalizado.magnitude <= 0.5f;
    }
}
