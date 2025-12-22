# 📐 Guide : Normaliser la position et taille des prefabs manuellement

## 🎯 Objectif
Faire en sorte que les **3 prefabs** (Temperature, FINAL_MODEL_GT4, FINAL_MODEL_A8) apparaissent **au même endroit** et avec **la même taille** en AR.

## 📝 Méthode manuelle (RECOMMANDÉE)

### Étape 1 : Ouvrir les prefabs dans Unity

1. Dans le **Project**, allez dans `Assets/Models/Prefabs/`
2. Vous verrez vos 3 prefabs :
   - `Temperature.prefab`
   - `FINAL_MODEL_GT4.prefab`
   - `FINAL_MODEL_A8.prefab`

### Étape 2 : Choisir un prefab de référence

Choisissez un prefab comme **référence** (celui qui a la bonne position/taille).
Par exemple : `Temperature.prefab`

1. **Double-cliquez** sur `Temperature.prefab` pour l'ouvrir en mode édition
2. Sélectionnez l'objet **racine** (root) du prefab dans la Hierarchy
3. Dans l'**Inspector**, notez les valeurs de **Transform** :
   - **Position** (X, Y, Z)
   - **Rotation** (X, Y, Z)
   - **Scale** (X, Y, Z)

### Étape 3 : Appliquer les mêmes valeurs aux autres prefabs

#### Pour FINAL_MODEL_GT4 :

1. **Double-cliquez** sur `FINAL_MODEL_GT4.prefab`
2. Sélectionnez l'objet **racine** dans la Hierarchy
3. Dans l'**Inspector**, section **Transform** :
   - Copiez les valeurs de **Position** de Temperature
   - Copiez les valeurs de **Rotation** de Temperature
   - Copiez les valeurs de **Scale** de Temperature
4. **Sauvegardez** (Ctrl+S / Cmd+S)
5. Fermez le prefab (clic sur la flèche ← en haut de la Hierarchy)

#### Pour FINAL_MODEL_A8 :

1. **Double-cliquez** sur `FINAL_MODEL_A8.prefab`
2. Répétez les mêmes étapes que pour GT4
3. **Sauvegardez** et fermez

## 📋 Valeurs recommandées pour AR

Pour des objets AR bien affichés, utilisez généralement :

| Paramètre | Valeur recommandée | Explication |
|-----------|-------------------|-------------|
| **Position** | `(0, 0, 0)` | Centre du marker AR |
| **Rotation** | `(0, 0, 0)` | Orientation standard |
| **Scale** | `(1, 1, 1)` ou ajusté | Échelle uniforme |

### Note sur l'échelle :
- Si vos modèles ont des tailles très différentes, vous devrez ajuster **Scale** individuellement
- Par exemple : `Temperature` à (1, 1, 1) et `GT4` à (0.5, 0.5, 0.5) si GT4 est 2x plus grand

## 🔍 Astuce : Copier/Coller les valeurs Transform

Unity permet de copier les composants :

### Méthode rapide :

1. **Ouvrez** le prefab de **référence** (ex: Temperature)
2. Dans l'Inspector, **clic droit** sur **Transform** (en haut)
3. Choisissez **"Copy Component"**
4. Fermez ce prefab

5. **Ouvrez** le prefab à modifier (ex: GT4)
6. **Clic droit** sur **Transform**
7. Choisissez **"Paste Component Values"**
8. Sauvegardez et fermez

✅ Répétez pour le 3ème prefab !

## 📊 Vérification : Sont-ils alignés ?

### Test dans l'éditeur :

1. Créez une **nouvelle scène** de test
2. Glissez les 3 prefabs dans la scène
3. Placez-les à `(0, 0, 0)` tous les trois
4. Vérifiez visuellement s'ils se superposent correctement
5. Si oui → ✅ C'est bon !
6. Si non → Ajustez les transforms

### Test en AR :

1. **Buildez** l'application Android
2. Testez avec votre marker AR
3. Utilisez le **bouton NEXT** pour basculer entre les prefabs
4. Vérifiez qu'ils apparaissent au même endroit

## 🎨 Ajuster la taille relative

Si un modèle est **trop grand** ou **trop petit** par rapport aux autres :

### Méthode 1 : Échelle uniforme

1. Ouvrez le prefab concerné
2. Modifiez **Scale** avec le **même facteur** pour X, Y, Z
   - Exemple : `(2, 2, 2)` pour doubler la taille
   - Exemple : `(0.5, 0.5, 0.5)` pour réduire de moitié

### Méthode 2 : Normaliser la taille visuelle

Si vous voulez que tous les modèles aient approximativement la **même taille visuelle** :

1. Ouvrez un prefab
2. Dans la **Scene view**, regardez sa taille
3. Ajustez **Scale** jusqu'à la taille souhaitée
4. Notez la valeur (ex: 0.8)
5. Répétez pour les autres prefabs

## 🚨 Problèmes courants

### Problème : Les modèles ne se superposent pas exactement

**Cause** : Les points d'origine (pivot) des modèles 3D sont différents

**Solution** :
1. Ouvrez le prefab
2. Si le modèle 3D est un enfant, sélectionnez-le
3. Ajustez sa **position locale** pour aligner le pivot
4. Ou dans Blender, recentrez le pivot avant d'exporter

### Problème : Un modèle est orienté différemment

**Cause** : Rotation différente à l'export depuis Blender

**Solution** :
1. Ouvrez le prefab
2. Sélectionnez l'objet 3D enfant
3. Ajustez sa **rotation locale**
4. Ou corrigez dans Blender et ré-exportez

### Problème : Les échelles sont trop différentes

**Cause** : Unités différentes lors de l'export (cm vs m)

**Solution** :
1. Dans l'Inspector du **fichier FBX** (pas le prefab)
2. Section **Model** → Vérifiez **Scale Factor**
3. Ajustez à une valeur cohérente (généralement 1 ou 100)
4. Cliquez **Apply**

## 📖 Exemple de workflow complet

### Exemple concret :

```
1. Ouvrir Temperature.prefab
   → Transform: Pos (0,0,0), Rot (0,0,0), Scale (1,1,1)
   → Clic droit sur Transform → Copy Component
   
2. Ouvrir FINAL_MODEL_GT4.prefab
   → Clic droit sur Transform → Paste Component Values
   → Ajuster Scale à (0.5, 0.5, 0.5) car trop grand
   → Sauvegarder (Ctrl+S)
   
3. Ouvrir FINAL_MODEL_A8.prefab
   → Clic droit sur Transform → Paste Component Values
   → Ajuster Scale à (0.8, 0.8, 0.8) car un peu grand
   → Sauvegarder (Ctrl+S)
   
4. Tester en AR
   → Build et install sur Android
   → Vérifier avec le bouton NEXT
```

## 🛠️ Option alternative : Script automatique

Si vous changez d'avis et voulez automatiser, le script `NormalizePrefabsTransform.cs` a été créé :
- Menu : **Tools → Prefabs → Normaliser Transform des Prefabs**
- Interface graphique pour définir les valeurs
- Applique automatiquement à tous les prefabs

Mais la **méthode manuelle reste plus simple** pour 3 prefabs ! 👍

## ✅ Checklist finale

- [ ] Les 3 prefabs ont la même **Position** (généralement 0,0,0)
- [ ] Les 3 prefabs ont la même **Rotation** (généralement 0,0,0)
- [ ] Les échelles (**Scale**) sont ajustées pour une taille visuelle cohérente
- [ ] Test en scène : les prefabs se superposent correctement
- [ ] Test en AR : le bouton NEXT change de modèle au même endroit

---

**Temps estimé** : 5-10 minutes  
**Difficulté** : ⭐ Facile  
**Résultat** : Prefabs parfaitement alignés en AR
