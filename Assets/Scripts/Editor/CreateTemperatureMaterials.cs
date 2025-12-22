using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Script Editor pour créer automatiquement les matériaux URP optimisés
/// pour le modèle Temperature_Blender sans ombres indésirables
/// </summary>
public class CreateTemperatureMaterials : EditorWindow
{
    /// <summary>
    /// Trouve le shader approprié pour le projet (URP ou Built-in)
    /// </summary>
    private static Shader FindBestShader()
    {
        // Liste de shaders à essayer par ordre de préférence
        string[] shaderNames = new string[]
        {
            "Universal Render Pipeline/Lit",
            "URP/Lit",
            "Shader Graphs/Lit",
            "Standard",
            "Mobile/Diffuse",
            "Diffuse"
        };

        foreach (string shaderName in shaderNames)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader != null)
            {
                Debug.Log($"✅ Shader trouvé : {shaderName}");
                return shader;
            }
        }

        // Si aucun shader n'a été trouvé, afficher les shaders disponibles
        Debug.LogError("❌ Aucun shader approprié trouvé ! Shaders disponibles :");
        var allShaders = Resources.FindObjectsOfTypeAll<Shader>();
        foreach (var shader in allShaders)
        {
            if (!shader.name.StartsWith("Hidden/"))
            {
                Debug.Log($"  - {shader.name}");
            }
        }

        return null;
    }

    [MenuItem("Tools/Matériaux/Créer Matériaux Temperature")]
    public static void CreateMaterials()
    {
        // Trouver le meilleur shader disponible
        Shader materialShader = FindBestShader();
        
        if (materialShader == null)
        {
            EditorUtility.DisplayDialog(
                "Erreur de shader",
                "Impossible de trouver un shader approprié.\n\nVérifiez la console pour voir les shaders disponibles.",
                "OK"
            );
            return;
        }

        // Définir le chemin de destination des matériaux
        string materialPath = "Assets/Models/Materials";
        
        // Créer le dossier s'il n'existe pas
        if (!Directory.Exists(materialPath))
        {
            Directory.CreateDirectory(materialPath);
            AssetDatabase.Refresh();
        }

        // Définir les matériaux à créer avec leurs couleurs
        string[] materialNames = new string[] 
        { 
            "Glass", 
            "Orange_PLA", 
            "Orange_PLA.001", 
            "Steel", 
            "TempDialGraphic", 
            "Yellow_PLA" 
        };

        // Couleurs associées (ajustables selon vos besoins)
        Color[] materialColors = new Color[]
        {
            new Color(0.8f, 0.9f, 1.0f, 0.3f),  // Glass - Transparent bleu clair
            new Color(1.0f, 0.5f, 0.1f, 1.0f),  // Orange_PLA
            new Color(1.0f, 0.6f, 0.2f, 1.0f),  // Orange_PLA.001 - Orange plus clair
            new Color(0.6f, 0.6f, 0.65f, 1.0f), // Steel - Gris métallique
            new Color(1.0f, 1.0f, 1.0f, 1.0f),  // TempDialGraphic - Blanc
            new Color(1.0f, 0.95f, 0.2f, 1.0f)  // Yellow_PLA
        };

        int createdCount = 0;
        bool isURPShader = materialShader.name.Contains("Universal") || materialShader.name.Contains("URP");

        for (int i = 0; i < materialNames.Length; i++)
        {
            string matName = materialNames[i];
            string fullPath = $"{materialPath}/{matName}.mat";

            // Vérifier si le matériau existe déjà
            Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            
            if (existingMat == null)
            {
                // Créer un nouveau matériau avec le shader trouvé
                Material newMaterial = new Material(materialShader);
                
                // Configurer les propriétés du matériau
                newMaterial.name = matName;
                newMaterial.color = materialColors[i];

                // Configurer pour éviter les ombres indésirables
                // Cette propriété fonctionne uniquement avec URP
                if (isURPShader)
                {
                    newMaterial.SetFloat("_ReceiveShadows", 0.0f);
                }
                
                // Configuration spécifique pour le verre (transparent)
                if (matName == "Glass")
                {
                    if (isURPShader)
                    {
                        // Configuration URP pour la transparence
                        newMaterial.SetFloat("_Surface", 1); // Transparent
                        newMaterial.SetFloat("_Blend", 0); // Alpha
                        newMaterial.SetFloat("_AlphaClip", 0);
                        newMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        newMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        newMaterial.SetFloat("_ZWrite", 0);
                        newMaterial.renderQueue = 3000;
                        newMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                        newMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    }
                    else
                    {
                        // Configuration Built-in pour la transparence
                        newMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        newMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        newMaterial.SetInt("_ZWrite", 0);
                        newMaterial.DisableKeyword("_ALPHATEST_ON");
                        newMaterial.EnableKeyword("_ALPHABLEND_ON");
                        newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        newMaterial.renderQueue = 3000;
                    }
                }
                // Configuration métallique pour Steel
                else if (matName == "Steel")
                {
                    if (isURPShader)
                    {
                        newMaterial.SetFloat("_Metallic", 0.8f);
                        newMaterial.SetFloat("_Smoothness", 0.7f);
                    }
                    else
                    {
                        // Pour le shader Standard Built-in
                        newMaterial.SetFloat("_Metallic", 0.8f);
                        newMaterial.SetFloat("_Glossiness", 0.7f);
                    }
                }
                // Configuration pour les PLA (plastique)
                else if (matName.Contains("PLA"))
                {
                    if (isURPShader)
                    {
                        newMaterial.SetFloat("_Metallic", 0.0f);
                        newMaterial.SetFloat("_Smoothness", 0.5f);
                    }
                    else
                    {
                        newMaterial.SetFloat("_Metallic", 0.0f);
                        newMaterial.SetFloat("_Glossiness", 0.5f);
                    }
                }

                // Sauvegarder le matériau
                AssetDatabase.CreateAsset(newMaterial, fullPath);
                createdCount++;
                
                Debug.Log($"✅ Matériau créé : {matName}");
            }
            else
            {
                Debug.Log($"ℹ️ Matériau existe déjà : {matName}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Création de matériaux", 
            $"Terminé !\n\n{createdCount} matériaux créés.\n{materialNames.Length - createdCount} matériaux existaient déjà.\n\nChemin: {materialPath}",
            "OK"
        );

        Debug.Log($"✨ Création terminée : {createdCount} nouveaux matériaux dans {materialPath}");
    }

    [MenuItem("Tools/Matériaux/Assigner Matériaux à Temperature_Blender")]
    public static void AssignMaterialsToModel()
    {
        // Charger le modèle FBX
        string modelPath = "Assets/Models/Prefabs/temperature_Blender.fbx";
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

        if (model == null)
        {
            EditorUtility.DisplayDialog("Erreur", $"Impossible de trouver le modèle à :\n{modelPath}", "OK");
            return;
        }

        string materialPath = "Assets/Models/Materials";
        
        // Obtenir tous les renderers du modèle
        MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>(true);
        
        int assignedCount = 0;

        foreach (MeshRenderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    string matName = materials[i].name.Replace(" (Instance)", "");
                    string matPath = $"{materialPath}/{matName}.mat";
                    
                    Material customMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    
                    if (customMat != null)
                    {
                        materials[i] = customMat;
                        assignedCount++;
                        Debug.Log($"✅ Matériau assigné : {matName} à {renderer.gameObject.name}");
                    }
                }
            }
            
            renderer.sharedMaterials = materials;
        }

        EditorUtility.SetDirty(model);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Attribution des matériaux",
            $"Terminé !\n\n{assignedCount} matériaux assignés au modèle Temperature_Blender.",
            "OK"
        );

        Debug.Log($"✨ Attribution terminée : {assignedCount} matériaux assignés");
    }

    [MenuItem("Tools/Matériaux/Désactiver Ombres Temperature_Blender")]
    public static void DisableShadowsOnModel()
    {
        string modelPath = "Assets/Models/Prefabs/temperature_Blender.fbx";
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

        if (model == null)
        {
            EditorUtility.DisplayDialog("Erreur", $"Impossible de trouver le modèle à :\n{modelPath}", "OK");
            return;
        }

        MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>(true);
        
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            EditorUtility.SetDirty(renderer);
        }

        SkinnedMeshRenderer[] skinnedRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        
        foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            EditorUtility.SetDirty(renderer);
        }

        EditorUtility.SetDirty(model);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Désactivation des ombres",
            $"Terminé !\n\nLes ombres ont été désactivées pour tous les renderers du modèle Temperature_Blender.",
            "OK"
        );

        Debug.Log("✨ Ombres désactivées sur Temperature_Blender");
    }
}
