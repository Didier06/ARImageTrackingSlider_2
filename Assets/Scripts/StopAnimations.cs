using UnityEngine;

public class StopAnimations : MonoBehaviour
{
    void Awake()
    {
        // TUE TOUT CE QUI BOUGE
        Animator[] animators = GetComponentsInChildren<Animator>(true);
        foreach (var anim in animators)
        {
            anim.enabled = false;
            // Optionnel : Détruire le composant si vraiment récalcitrant
            // Destroy(anim); 
        }

        Animation[] animations = GetComponentsInChildren<Animation>(true);
        foreach (var anim in animations)
        {
            anim.enabled = false;
            anim.Stop();
        }
    }
}
