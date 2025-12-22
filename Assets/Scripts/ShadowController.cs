using UnityEngine;

/// <summary>
/// Script pour contrôler les ombres sur les objets 3D importés
/// Utile pour désactiver les ombres indésirables sur mobile/AR
/// </summary>
public class ShadowController : MonoBehaviour
{
    [Header("Paramètres des ombres")]
    [Tooltip("Activer/Désactiver la projection d'ombres")]
    public bool castShadows = false;
    
    [Tooltip("Activer/Désactiver la réception d'ombres")]
    public bool receiveShadows = false;
    
    [Header("Appliquer aux enfants")]
    [Tooltip("Appliquer les paramètres à tous les mesh renderers enfants")]
    public bool applyToChildren = true;

    void Start()
    {
        ApplyShadowSettings();
    }

    /// <summary>
    /// Applique les paramètres d'ombres à tous les Mesh Renderers
    /// </summary>
    public void ApplyShadowSettings()
    {
        if (applyToChildren)
        {
            // Appliquer à tous les mesh renderers de cet objet et ses enfants
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in meshRenderers)
            {
                SetShadowMode(renderer);
            }
            
            // Également pour les Skinned Mesh Renderers (modèles animés)
            SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
            {
                SetShadowMode(renderer);
            }
        }
        else
        {
            // Appliquer uniquement au Mesh Renderer de cet objet
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                SetShadowMode(meshRenderer);
            }
            
            SkinnedMeshRenderer skinnedRenderer = GetComponent<SkinnedMeshRenderer>();
            if (skinnedRenderer != null)
            {
                SetShadowMode(skinnedRenderer);
            }
        }
    }

    /// <summary>
    /// Configure le mode d'ombres pour un renderer
    /// </summary>
    private void SetShadowMode(Renderer renderer)
    {
        if (renderer != null)
        {
            // Définir si l'objet projette des ombres
            renderer.shadowCastingMode = castShadows 
                ? UnityEngine.Rendering.ShadowCastingMode.On 
                : UnityEngine.Rendering.ShadowCastingMode.Off;
            
            // Définir si l'objet reçoit des ombres
            renderer.receiveShadows = receiveShadows;
            
            Debug.Log($"Paramètres d'ombres appliqués à {renderer.gameObject.name}: Cast={castShadows}, Receive={receiveShadows}");
        }
    }

    // Méthode appelée dans l'éditeur quand les valeurs changent
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyShadowSettings();
        }
    }
}
