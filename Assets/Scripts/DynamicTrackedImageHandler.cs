using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class DynamicTrackedImageHandler : MonoBehaviour
{
    public ARTrackedImageManager imageManager;
    public GameObject[] prefabs;
    
    private int currentPrefabIndex = 0;
    private Dictionary<string, GameObject> spawnedPrefabs = new();

    void Start()
    {
        Debug.Log("=== STARTRUNNING: DynamicTrackedImageHandler ===");
        
        if (imageManager == null)
        {
            imageManager = GetComponent<ARTrackedImageManager>();
            if (imageManager == null)
            {
                Debug.LogError("CRITICAL: Image Manager not found!");
                return;
            }
        }
        
        // Nettoyage et abonnement propre
        imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        imageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        
        Debug.Log("SUCCES: Listener attached via Start(). Prefabs count: " + (prefabs != null ? prefabs.Length : 0));
    }

    // Méthode sécurisée pour obtenir le nom de l'image (évite le ArgumentNullException)
    private string GetSafeName(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return "NULL_IMAGE";
        if (trackedImage.referenceImage == null) return "NULL_REF_" + trackedImage.trackableId;
        if (string.IsNullOrEmpty(trackedImage.referenceImage.name)) return "UNNAMED_" + trackedImage.referenceImage.guid;
        
        return trackedImage.referenceImage.name;
    }

    void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var trackedImage in args.added)
        {
            UpdateImage(trackedImage);
        }

        foreach (var trackedImage in args.updated)
        {
            UpdateImage(trackedImage);
        }

        // Gestion compatible des suppressions
        /* 
        Note: args.removed peut varier selon la version d'ARFoundation.
        On le commente temporairement pour éviter l'erreur de compilation,
        car la logique principale (affichage/changement) ne dépend pas de ça.
        */
        /*
        foreach (var trackedImage in args.removed)
        {
             // Code de suppression désactivé pour compiler
        }
        */
    }

    private void UpdateImage(ARTrackedImage trackedImage)
    {
        string imageName = GetSafeName(trackedImage);

        // Si l'image est bien suivie (Tracking)
        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            // Vérifier si le prefab existe déjà
            if (spawnedPrefabs.TryGetValue(imageName, out GameObject existingPrefab))
            {
                // Activez-le si nécessaire
                if (!existingPrefab.activeSelf) existingPrefab.SetActive(true);
                
                // Positionnement
                // Modification : On laisse le système de parenté gérer la position/rotation
                // existingPrefab.transform.position = trackedImage.transform.position;
                // existingPrefab.transform.rotation = trackedImage.transform.rotation;
                
                // On s'assure juste qu'il est bien enfant (au cas où il aurait été détaché)
                if (existingPrefab.transform.parent != trackedImage.transform)
                {
                   existingPrefab.transform.SetParent(trackedImage.transform);
                }
            }
            else
            {
                // Pas encore de prefab -> On instancie le courant
                SpawnPrefabForImage(imageName, trackedImage);
            }
        }
        else // Limited ou None
        {
            // On cache le prefab
            if (spawnedPrefabs.TryGetValue(imageName, out GameObject existingPrefab))
            {
                if (existingPrefab.activeSelf) existingPrefab.SetActive(false);
            }
        }
    }

    private void SpawnPrefabForImage(string imageName, ARTrackedImage trackedImage)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        // Choix du prefab
        int index = currentPrefabIndex % prefabs.Length;
        GameObject prefabToSpawn = prefabs[index];

        if (prefabToSpawn != null)
        {
            // Instantiation
            GameObject newPrefab = Instantiate(prefabToSpawn, trackedImage.transform.position, trackedImage.transform.rotation);
            
            // PARENTAGE CRITIQUE : Permet aux sliders/MQTT de modifier transform.localPosition/Rotation 
            // tout en suivant le marker.
            newPrefab.transform.SetParent(trackedImage.transform);
            
            spawnedPrefabs[imageName] = newPrefab;
            Debug.Log($"[SPAWN] Prefab '{newPrefab.name}' créé pour {imageName} (Index: {index})");
        }
    }

    // Fonction appelée par le bouton NEXT
    public void SwitchToNextPrefab()
    {
        Debug.Log("=== BOUTON NEXT CLIQUÉ ===");
        
        if (prefabs == null || prefabs.Length == 0) return;

        // Passer à l'index suivant
        currentPrefabIndex++;
        if (currentPrefabIndex >= prefabs.Length) currentPrefabIndex = 0;

        Debug.Log($"[SWITCH] Nouvel index : {currentPrefabIndex}");

        // On détruit TOUS les prefabs actuellement instanciés
        foreach (var obj in spawnedPrefabs.Values)
        {
            if (obj != null) Destroy(obj);
        }
        
        // On vide le dictionnaire
        spawnedPrefabs.Clear();
        
        Debug.Log("[SWITCH] Tout détruit. Le prochain Update recréera le bon prefab automatiquement.");
    }

    // Méthode de compatibilité pour MQTTManager (avec ou sans argument)
    public GameObject GetSpawnedInstance(string imageName = null)
    {
        // Si un nom précis est demandé
        if (!string.IsNullOrEmpty(imageName) && spawnedPrefabs.TryGetValue(imageName, out GameObject specificPrefab))
        {
            return specificPrefab;
        }

        // Sinon retourne le premier actif
        foreach (var kvp in spawnedPrefabs)
        {
            if (kvp.Value != null && kvp.Value.activeSelf) return kvp.Value;
        }
        return null;
    }
}
