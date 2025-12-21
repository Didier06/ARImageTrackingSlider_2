using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// Script simplifié pour afficher UN SEUL prefab sur les images AR détectées
/// Pas de bouton Next, pas de complications - juste afficher le prefab
/// </summary>
public class SimplifiedTrackedImageHandler : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Le ARTrackedImageManager de votre scène")]
    public ARTrackedImageManager imageManager;
    
    [Tooltip("Le prefab à afficher (FINAL_Model8A8)")]
    public GameObject prefabToDisplay;

    // Dictionnaire pour garder trace des objets instanciés
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    void OnEnable()
    {
        // Vérification de sécurité
        if (imageManager == null)
        {
            Debug.LogError("[ERREUR] ARTrackedImageManager n'est pas assigne dans l'Inspector!");
            return;
        }

        if (prefabToDisplay == null)
        {
            Debug.LogError("[ERREUR] Prefab To Display n'est pas assigne dans l'Inspector!");
            return;
        }

        // S'abonner aux événements AR
        imageManager.trackablesChanged.AddListener(OnImageChanged);
        Debug.Log("[OK] SimplifiedTrackedImageHandler active");
    }

    void OnDisable()
    {
        if (imageManager != null)
        {
            imageManager.trackablesChanged.RemoveListener(OnImageChanged);
        }
    }

    void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Images nouvellement detectees
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            if (trackedImage.referenceImage == null)
            {
                Debug.LogWarning("[WARN] Image trackee sans reference image");
                continue;
            }

            string imageName = trackedImage.referenceImage.name;
            
            if (string.IsNullOrEmpty(imageName))
            {
                Debug.LogWarning("[WARN] Image trackee avec nom null ou vide");
                continue;
            }

            Debug.Log("[NOUVELLE IMAGE] " + imageName);

            // Creer l'objet et l'attacher a l'image
            GameObject newObject = Instantiate(prefabToDisplay, trackedImage.transform);
            spawnedObjects[imageName] = newObject;

            Debug.Log("[PREFAB AFFICHE] " + imageName);
        }

        // Images mises a jour (changement d'etat)
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            if (trackedImage.referenceImage == null || string.IsNullOrEmpty(trackedImage.referenceImage.name))
                continue;

            string imageName = trackedImage.referenceImage.name;

            if (spawnedObjects.ContainsKey(imageName))
            {
                GameObject obj = spawnedObjects[imageName];
                
                // Activer/desactiver selon l'etat de tracking
                bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
                obj.SetActive(isTracking);

                string status = isTracking ? "Active" : "Desactive";
                Debug.Log("[UPDATE] " + imageName + ": " + status + " (State: " + trackedImage.trackingState + ")");
            }
        }

        // Images supprimees
        foreach (var kvp in eventArgs.removed)
        {
            ARTrackedImage trackedImage = kvp.Value;
            
            if (trackedImage.referenceImage == null || string.IsNullOrEmpty(trackedImage.referenceImage.name))
                continue;

            string imageName = trackedImage.referenceImage.name;

            if (spawnedObjects.ContainsKey(imageName))
            {
                spawnedObjects[imageName].SetActive(false);
                Debug.Log("[REMOVED] " + imageName + " supprime (desactive)");
            }
        }
    }
}
