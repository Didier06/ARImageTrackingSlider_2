using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Script Editor pour normaliser la position, rotation et échelle de plusieurs prefabs
/// afin qu'ils apparaissent tous au même endroit et avec la même taille en AR
/// </summary>
public class NormalizePrefabsTransform : EditorWindow
{
    [Header("Prefabs à normaliser")]
    private List<GameObject> prefabs = new List<GameObject>();
    
    [Header("Paramètres de normalisation")]
    private Vector3 targetPosition = Vector3.zero;
    private Vector3 targetRotation = Vector3.zero;
    private Vector3 targetScale = Vector3.one;
    
    [Header("Options")]
    private bool normalizePosition = true;
    private bool normalizeRotation = true;
    private bool normalizeScale = true;
    private bool applyToChildren = false;
    
    [MenuItem("Tools/Prefabs/Normaliser Transform des Prefabs")]
    public static void ShowWindow()
    {
        var window = GetWindow<NormalizePrefabsTransform>("Normaliser Prefabs");
        window.minSize = new Vector2(450, 500);
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Normalisation des Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Cet outil permet de normaliser la position, rotation et échelle de plusieurs prefabs " +
            "pour qu'ils apparaissent tous au même endroit et avec la même taille.", 
            MessageType.Info
        );
        
        EditorGUILayout.Space(10);
        
        // Section : Prefabs à normaliser
        EditorGUILayout.LabelField("Prefabs à normaliser", EditorStyles.boldLabel);
        
        // Bouton pour charger automatiquement les prefabs principaux
        if (GUILayout.Button("Charger les prefabs Temperature, GT4, A8"))
        {
            LoadMainPrefabs();
        }
        
        EditorGUILayout.Space(5);
        
        // Liste des prefabs
        int newCount = Mathf.Max(0, EditorGUILayout.IntField("Nombre de prefabs", prefabs.Count));
        while (newCount < prefabs.Count)
            prefabs.RemoveAt(prefabs.Count - 1);
        while (newCount > prefabs.Count)
            prefabs.Add(null);

        for (int i = 0; i < prefabs.Count; i++)
        {
            prefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab {i + 1}", prefabs[i], typeof(GameObject), false);
        }
        
        EditorGUILayout.Space(10);
        
        // Section : Paramètres de normalisation
        EditorGUILayout.LabelField("Paramètres cibles", EditorStyles.boldLabel);
        
        normalizePosition = EditorGUILayout.Toggle("Normaliser Position", normalizePosition);
        if (normalizePosition)
        {
            EditorGUI.indentLevel++;
            targetPosition = EditorGUILayout.Vector3Field("Position", targetPosition);
            EditorGUI.indentLevel--;
        }
        
        normalizeRotation = EditorGUILayout.Toggle("Normaliser Rotation", normalizeRotation);
        if (normalizeRotation)
        {
            EditorGUI.indentLevel++;
            targetRotation = EditorGUILayout.Vector3Field("Rotation (Euler)", targetRotation);
            EditorGUI.indentLevel--;
        }
        
        normalizeScale = EditorGUILayout.Toggle("Normaliser Échelle", normalizeScale);
        if (normalizeScale)
        {
            EditorGUI.indentLevel++;
            targetScale = EditorGUILayout.Vector3Field("Échelle", targetScale);
            
            // Boutons rapides pour échelle uniforme
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("x0.5")) targetScale = Vector3.one * 0.5f;
            if (GUILayout.Button("x1")) targetScale = Vector3.one;
            if (GUILayout.Button("x2")) targetScale = Vector3.one * 2f;
            if (GUILayout.Button("x5")) targetScale = Vector3.one * 5f;
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(5);
        applyToChildren = EditorGUILayout.Toggle("Appliquer aux enfants", applyToChildren);
        
        EditorGUILayout.Space(15);
        
        // Boutons d'action
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("✓ APPLIQUER LA NORMALISATION", GUILayout.Height(40)))
        {
            NormalizePrefabs();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("Analyser les différences actuelles"))
        {
            AnalyzePrefabDifferences();
        }
        
        EditorGUILayout.Space(10);
        
        // Section : Presets rapides
        EditorGUILayout.LabelField("Presets rapides", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset complet (0,0,0 / 0,0,0 / 1,1,1)"))
        {
            targetPosition = Vector3.zero;
            targetRotation = Vector3.zero;
            targetScale = Vector3.one;
            normalizePosition = true;
            normalizeRotation = true;
            normalizeScale = true;
        }
        
        if (GUILayout.Button("Seulement échelle x1"))
        {
            targetScale = Vector3.one;
            normalizePosition = false;
            normalizeRotation = false;
            normalizeScale = true;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void LoadMainPrefabs()
    {
        prefabs.Clear();
        
        string[] prefabPaths = new string[]
        {
            "Assets/Models/Prefabs/Temperature.prefab",
            "Assets/Models/Prefabs/FINAL_MODEL_GT4.prefab",
            "Assets/Models/Prefabs/FINAL_MODEL_A8.prefab"
        };
        
        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                prefabs.Add(prefab);
                Debug.Log($"✅ Prefab chargé : {prefab.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Prefab non trouvé : {path}");
            }
        }
        
        if (prefabs.Count > 0)
        {
            EditorUtility.DisplayDialog(
                "Prefabs chargés",
                $"{prefabs.Count} prefab(s) chargé(s) avec succès.",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Erreur",
                "Aucun prefab n'a pu être chargé. Vérifiez les chemins.",
                "OK"
            );
        }
    }

    private void AnalyzePrefabDifferences()
    {
        if (prefabs.Count == 0)
        {
            EditorUtility.DisplayDialog("Erreur", "Aucun prefab sélectionné.", "OK");
            return;
        }
        
        Debug.Log("=== ANALYSE DES DIFFÉRENCES ===");
        
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
            {
                Debug.Log($"\n📦 {prefab.name}:");
                Debug.Log($"   Position: {prefab.transform.localPosition}");
                Debug.Log($"   Rotation: {prefab.transform.localEulerAngles}");
                Debug.Log($"   Échelle:  {prefab.transform.localScale}");
                
                // Analyser les bounds pour comprendre la taille réelle
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds combinedBounds = renderers[0].bounds;
                    foreach (Renderer r in renderers)
                    {
                        combinedBounds.Encapsulate(r.bounds);
                    }
                    Debug.Log($"   Taille réelle (bounds): {combinedBounds.size}");
                }
            }
        }
        
        Debug.Log("\n=== FIN DE L'ANALYSE ===");
        
        EditorUtility.DisplayDialog(
            "Analyse terminée",
            "Les informations ont été affichées dans la Console.\n\n" +
            "Window → General → Console (Ctrl+Shift+C)",
            "OK"
        );
    }

    private void NormalizePrefabs()
    {
        if (prefabs.Count == 0)
        {
            EditorUtility.DisplayDialog("Erreur", "Aucun prefab sélectionné.", "OK");
            return;
        }
        
        int modifiedCount = 0;
        
        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null) continue;
            
            // Ouvrir le prefab pour modification
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
            
            if (prefabInstance != null)
            {
                // Appliquer les transformations
                if (normalizePosition)
                {
                    prefabInstance.transform.localPosition = targetPosition;
                }
                
                if (normalizeRotation)
                {
                    prefabInstance.transform.localEulerAngles = targetRotation;
                }
                
                if (normalizeScale)
                {
                    prefabInstance.transform.localScale = targetScale;
                }
                
                // Si appliquer aux enfants
                if (applyToChildren)
                {
                    Transform[] children = prefabInstance.GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in children)
                    {
                        if (child == prefabInstance.transform) continue; // Skip root
                        
                        if (normalizePosition) child.localPosition = targetPosition;
                        if (normalizeRotation) child.localEulerAngles = targetRotation;
                        if (normalizeScale) child.localScale = targetScale;
                    }
                }
                
                // Sauvegarder les modifications
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabInstance);
                
                modifiedCount++;
                Debug.Log($"✅ Prefab normalisé : {prefab.name}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        string summary = $"Normalisation terminée !\n\n" +
                        $"• {modifiedCount} prefab(s) modifié(s)\n";
        
        if (normalizePosition) summary += $"• Position : {targetPosition}\n";
        if (normalizeRotation) summary += $"• Rotation : {targetRotation}\n";
        if (normalizeScale) summary += $"• Échelle : {targetScale}\n";
        
        EditorUtility.DisplayDialog("Succès", summary, "OK");
        
        Debug.Log($"✨ Normalisation terminée : {modifiedCount} prefab(s) modifié(s)");
    }
}
