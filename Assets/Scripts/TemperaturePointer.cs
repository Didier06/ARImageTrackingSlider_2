using UnityEngine;

/// <summary>
/// Contrôle la rotation de l'aiguille du thermomètre en fonction de la température reçue via MQTT.
/// Attachez ce script sur la racine du prefab temperature_Blender.
/// </summary>
public class PrefabTest : MonoBehaviour
{
    private Transform pointer;

    void OnEnable()
    {
        // Trouver l'aiguille automatiquement
        var allChildren = GetComponentsInChildren<Transform>(true);
        foreach (var child in allChildren)
        {
            if (child.name.IndexOf("Pointer", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                pointer = child;
                break;
            }
        }

        if (pointer != null)
        {
            StartCoroutine(UpdateTemperature());
        }
    }

    private System.Collections.IEnumerator UpdateTemperature()
    {
        var mqttManager = FindObjectOfType<MQTTManager>();
        
        if (mqttManager == null)
        {
            yield break;
        }

        // Boucle de mise à jour continue
        while (true)
        {
            var cmd = mqttManager.GetLastCommand();
            
            if (cmd != null)
            {
                // Formule : 30°C max, rotation sur axe Y
                float angle = -cmd.temperature * 180f / 30f;
                pointer.localEulerAngles = new Vector3(0f, angle, 0f);
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
}
