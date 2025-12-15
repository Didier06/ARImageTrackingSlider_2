using UnityEngine;

public class RotationAlternee : MonoBehaviour
{
    //public Transform objectToRotate;  // L'objet à faire tourner
    public float rotationSpeed = 5f; // degrés par seconde
    public int numberOfCycles = 6;

    private int completedCycles = 0;
    private float currentRotation = 0f;
    private int direction = 1; // 1 = sens horaire, -1 = sens inverse
    private bool isRotating = true;

    void Update()
    {
        //if (!isRotating || objectToRotate == null)
        if (!isRotating)
            return;

        float deltaRotation = rotationSpeed * Time.deltaTime * direction;
        //objectToRotate.Rotate(0, deltaRotation, 0);
        transform.Rotate(0, deltaRotation, 0);
        currentRotation += Mathf.Abs(deltaRotation);

        if (currentRotation >= 360f)
        {
            currentRotation = 0f;
            direction *= -1; // Inverser le sens
            completedCycles++;

            if (completedCycles >= numberOfCycles)
            {
                isRotating = false; // Arrêter après 5 cycles
            }
        }
    }
}
