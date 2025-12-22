# ARImageTrackingSlider_2
unity 6.3 ARfoundation 6.3 slider mqtt

pour commander la rotation ,....  : 
{
  "scale": 5,
  "rot": 40,
  "temperature": 25.0
}

sur le topic :  FABLAB_21_22/unity/test/in

![Aperçu du projet](images/Screenshot_ARImageTracking2.jpg)

## 📚 Documentation

### Guides de résolution de problèmes

- **[Guide des Ombres](GUIDE_OMBRES.md)** : Solutions pour éliminer les ombres indésirables sur Android
- **[Guide Temperature_Blender](GUIDE_TEMPERATURE_BLENDER.md)** : Configuration spécifique du prefab Temperature avec création automatique des matériaux
- **[Dépannage Shader](TROUBLESHOOTING_SHADER.md)** : Résoudre l'erreur "ArgumentNullException: shader"
- **[Normalisation des Prefabs](GUIDE_NORMALISATION_PREFABS.md)** : Aligner position et taille des 3 prefabs manuellement

### Scripts utilitaires

- **ShadowController.cs** : Contrôle des ombres par objet
- **CreateTemperatureMaterials.cs** : Création automatique des matériaux URP pour Temperature_Blender (Menu Unity : Tools → Matériaux)
  - ✅ Détection automatique du shader (URP ou Built-in)
  - ✅ Configuration adaptative selon le pipeline de rendu
- **NormalizePrefabsTransform.cs** : Outil optionnel pour normaliser automatiquement les prefabs (Menu Unity : Tools → Prefabs)
