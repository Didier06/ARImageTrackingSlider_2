using UnityEngine;

public class Rotationlente : MonoBehaviour
{
    public float rotationSpeed = 3.0f; // degrés/seconde
    private bool isRotating = true; // rotation active par défaut

    void Update()
    {
        if (isRotating)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        }
    }

    // Fonction appelée par le bouton
    public void ToggleRotation()
    {
        isRotating = !isRotating;
    }
}
