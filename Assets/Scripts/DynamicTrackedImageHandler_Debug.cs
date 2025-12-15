using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class DynamicTrackedImageHandler_Debug : MonoBehaviour
{
    public ARTrackedImageManager imageManager;
    public GameObject[] prefabs;
    private int currentPrefabIndex = 0;
    private Dictionary<string, GameObject> spawnedPrefabs = new();

    void Start()
    {
        Debug.Log("========== DÉMARRAGE DEBUG AR ==========");
        
        // Vérifier que le ImageManager est assigné
        if (imageManager == null)
        {
            Debug.LogError("❌ ERREUR CRITIQUE: ARTrackedImageManager n'est PAS assigné dans l'Inspector!");
            return;
        }
        else
        {
            Debug.Log("✅ ARTrackedImageManager est assigné");
        }

        // Vérifier que les prefabs sont assignés
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("❌ ERREUR CRITIQUE: Aucun prefab n'est assigné dans le tableau Prefabs!");
            return;
        }
        else
        {
            Debug.Log($"✅ {prefabs.Length} prefab(s) trouvé(s):");
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i] != null)
                {
                    Debug.Log($"  [{i}] {prefabs[i].name}");
                }
                else
                {
                    Debug.LogWarning($"  [{i}] ⚠️ Prefab NULL - emplacement vide!");
                }
            }
        }

        // Vérifier la bibliothèque d'images
        if (imageManager.referenceLibrary == null)
        {
            Debug.LogError("❌ ERREUR CRITIQUE: Reference Image Library n'est PAS assignée au ARTrackedImageManager!");
            return;
        }
        else
        {
            Debug.Log("✅ Reference Library assignée");
            Debug.Log($"   Nombre d'images dans la bibliothèque: {imageManager.referenceLibrary.count}");
            
            // Afficher les noms des images
            for (int i = 0; i < imageManager.referenceLibrary.count; i++)
            {
                Debug.Log($"   Image [{i}]: {imageManager.referenceLibrary[i].name}");
            }
        }
        
        Debug.Log("========================================");
    }

    void OnEnable()
    {
        if (imageManager != null)
        {
            imageManager.trackablesChanged.AddListener(OnTrackablesChanged);
            Debug.Log("✅ OnEnable: Abonnement à trackablesChanged (AR Foundation 5.0+)");
        }
        else
        {
            Debug.LogError("❌ OnEnable: imageManager est NULL!");
        }
    }

    void OnDisable()
    {
        if (imageManager != null)
        {
            imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
            Debug.Log("OnDisable: Désabonnement de trackablesChanged");
        }
    }

    void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        Debug.Log($"========== EVENT: TrackablesChanged (AR Foundation 5.0+) ==========");
        Debug.Log($"Images ajoutées: {args.added.Count}");
        Debug.Log($"Images mises à jour: {args.updated.Count}");
        Debug.Log($"Images supprimées: {args.removed.Count}");

        // Images ajoutées (nouvellement détectées)
        foreach (var trackedImage in args.added)
        {
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"🆕 NOUVELLE IMAGE DÉTECTÉE: {imageName}");
            Debug.Log($"   Position: {trackedImage.transform.position}");
            Debug.Log($"   Tracking State: {trackedImage.trackingState}");

            if (prefabs == null || prefabs.Length == 0)
            {
                Debug.LogError("❌ Impossible d'instancier: tableau prefabs vide!");
                continue;
            }

            if (currentPrefabIndex >= prefabs.Length)
            {
                Debug.LogError($"❌ Index prefab invalide: {currentPrefabIndex} >= {prefabs.Length}");
                continue;
            }

            if (prefabs[currentPrefabIndex] == null)
            {
                Debug.LogError($"❌ Le prefab à l'index {currentPrefabIndex} est NULL!");
                continue;
            }

            // Supprimer l'ancienne instance si elle existe
            if (spawnedPrefabs.ContainsKey(imageName))
            {
                Debug.Log($"   Destruction de l'ancienne instance pour {imageName}");
                Destroy(spawnedPrefabs[imageName]);
            }

            // Instancier le nouveau prefab
            Debug.Log($"   Instanciation du prefab: {prefabs[currentPrefabIndex].name}");
            GameObject instance = Instantiate(prefabs[currentPrefabIndex], trackedImage.transform);
            spawnedPrefabs[imageName] = instance;
            
            Debug.Log($"✅ SUCCÈS: Prefab instancié et attaché à {imageName}");
            Debug.Log($"   Instance active: {instance.activeSelf}");
            Debug.Log($"   Position locale: {instance.transform.localPosition}");
            Debug.Log($"   Échelle locale: {instance.transform.localScale}");
        }

        // Images mises à jour (déjà trackées, changement d'état)
        foreach (var trackedImage in args.updated)
        {
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"🔄 IMAGE MISE À JOUR: {imageName}");
            Debug.Log($"   Tracking State: {trackedImage.trackingState}");
            Debug.Log($"   Position: {trackedImage.transform.position}");

            // Activer/désactiver l'objet selon l'état de tracking
            if (spawnedPrefabs.ContainsKey(imageName) && spawnedPrefabs[imageName] != null)
            {
                bool shouldBeActive = trackedImage.trackingState == TrackingState.Tracking;
                spawnedPrefabs[imageName].SetActive(shouldBeActive);
                Debug.Log($"   Objet {(shouldBeActive ? "activé" : "désactivé")}");
            }
        }

        // Images supprimées (plus détectées)
        foreach (var kvp in args.removed)
        {
            ARTrackedImage trackedImage = kvp.Value;
            string imageName = trackedImage.referenceImage.name;
            Debug.Log($"🗑️ IMAGE SUPPRIMÉE: {imageName}");
            
            if (spawnedPrefabs.ContainsKey(imageName) && spawnedPrefabs[imageName] != null)
            {
                spawnedPrefabs[imageName].SetActive(false);
                Debug.Log($"   Objet désactivé");
            }
        }

        Debug.Log("===========================================");
    }

    public void SwitchToNextPrefab()
    {
        Debug.Log("========== CHANGEMENT DE PREFAB ==========");
        
        currentPrefabIndex = (currentPrefabIndex + 1) % prefabs.Length;
        Debug.Log($"Nouveau prefab actif: [{currentPrefabIndex}] {prefabs[currentPrefabIndex].name}");

        // Mettre à jour tous les objets déjà visibles
        int updateCount = 0;
        foreach (var kvp in spawnedPrefabs)
        {
            string imageName = kvp.Key;
            GameObject oldInstance = kvp.Value;

            if (oldInstance != null)
            {
                Debug.Log($"   Destruction de {oldInstance.name} pour {imageName}");
                Destroy(oldInstance);
            }

            ARTrackedImage trackedImage = FindTrackedImageByName(imageName);
            if (trackedImage != null && trackedImage.trackingState == TrackingState.Tracking)
            {
                GameObject newInstance = Instantiate(prefabs[currentPrefabIndex], trackedImage.transform);
                spawnedPrefabs[imageName] = newInstance;
                updateCount++;
                Debug.Log($"   ✅ Nouveau prefab instancié pour {imageName}");
            }
            else
            {
                Debug.LogWarning($"   ⚠️ Image {imageName} non trouvée ou non trackée");
            }
        }

        Debug.Log($"Total d'objets mis à jour: {updateCount}");
        Debug.Log("==========================================");
    }

    private ARTrackedImage FindTrackedImageByName(string name)
    {
        foreach (ARTrackedImage trackedImage in imageManager.trackables)
        {
            if (trackedImage.referenceImage.name == name)
                return trackedImage;
        }
        return null;
    }

    public GameObject GetSpawnedInstance(string imageName)
    {
        if (spawnedPrefabs.TryGetValue(imageName, out GameObject instance))
        {
            return instance;
        }
        return null;
    }

    // Méthode utile pour debug en temps réel
    void Update()
    {
        // Appuyez sur la touche D pour afficher l'état actuel
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("========== ÉTAT ACTUEL DEBUG ==========");
            Debug.Log($"Prefab actif: [{currentPrefabIndex}] {(prefabs != null && currentPrefabIndex < prefabs.Length ? prefabs[currentPrefabIndex].name : "INVALIDE")}");
            Debug.Log($"Objets instanciés: {spawnedPrefabs.Count}");
            
            if (imageManager != null)
            {
                Debug.Log($"Images trackées: {imageManager.trackables.count}");
                foreach (ARTrackedImage img in imageManager.trackables)
                {
                    Debug.Log($"   - {img.referenceImage.name}: {img.trackingState}");
                }
            }
            Debug.Log("========================================");
        }
    }
}
