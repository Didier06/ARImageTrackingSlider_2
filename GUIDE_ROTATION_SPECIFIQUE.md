# 🔄 Rotations spécifiques par prefab

## 📝 Modification appliquée

### Fichier modifié : `DynamicTrackedImageHandler.cs`

Une rotation supplémentaire de **-30.808°** sur l'axe **X** a été ajoutée **uniquement pour le prefab Temperature** au moment de son instanciation.

## 🎯 Comment ça fonctionne

### Code ajouté (lignes 122-129) :

```csharp
// Rotation supplémentaire spécifique pour le prefab Temperature
if (prefabToSpawn.name.Contains("Temperature"))
{
    // Ajouter une rotation de -30.808° sur l'axe X
    newPrefab.transform.Rotate(-30.808f, 0f, 0f, Space.Self);
    Debug.Log($"[SPAWN] Rotation X -30.808° appliquée au prefab Temperature");
}
```

### Explication :

1. **Après l'instanciation** du prefab
2. **Vérification** : Si le nom du prefab contient "Temperature"
3. **Application** : Rotation de -30.808° sur X (dans l'espace local)
4. **Log** : Message dans la console pour confirmer

## 🔍 Ordre d'application des rotations

Pour le prefab **Temperature**, voici l'ordre :

```
1. Rotation du marker AR (détectée automatiquement)
   ↓
2. Rotation du prefab (définie dans Temperature.prefab)
   ↓
3. Rotation supplémentaire via script (-30.808° sur X) ← NOUVEAU
   ↓
= Rotation finale visible dans l'application
```

Pour les autres prefabs (**GT4, A8**) :

```
1. Rotation du marker AR
   ↓
2. Rotation du prefab
   ↓
= Rotation finale (pas de rotation supplémentaire)
```

## ⚙️ Ajuster la valeur de rotation

### Pour modifier l'angle :

1. Ouvrez `DynamicTrackedImageHandler.cs`
2. Ligne 125, modifiez la valeur :
   ```csharp
   newPrefab.transform.Rotate(-30.808f, 0f, 0f, Space.Self);
                      // ↑ Changez cette valeur
   ```

### Exemples d'ajustements :

| Besoin | Valeur à utiliser |
|--------|------------------|
| Incliner plus vers l'avant | `-40f, 0f, 0f` |
| Incliner moins | `-20f, 0f, 0f` |
| Incliner vers l'arrière | `30f, 0f, 0f` (positif) |
| Rotation horizontale (Y) | `0f, 90f, 0f` |
| Rotation latérale (Z) | `0f, 0f, 45f` |

## 🎨 Ajouter des rotations pour d'autres prefabs

### Exemple : Ajouter une rotation pour GT4

```csharp
// Rotation supplémentaire spécifique pour le prefab Temperature
if (prefabToSpawn.name.Contains("Temperature"))
{
    newPrefab.transform.Rotate(-30.808f, 0f, 0f, Space.Self);
    Debug.Log($"[SPAWN] Rotation X -30.808° appliquée au prefab Temperature");
}
// Rotation pour GT4
else if (prefabToSpawn.name.Contains("GT4"))
{
    newPrefab.transform.Rotate(0f, 45f, 0f, Space.Self);
    Debug.Log($"[SPAWN] Rotation Y 45° appliquée au prefab GT4");
}
// Rotation pour A8
else if (prefabToSpawn.name.Contains("A8"))
{
    newPrefab.transform.Rotate(10f, 0f, 0f, Space.Self);
    Debug.Log($"[SPAWN] Rotation X 10° appliquée au prefab A8");
}
```

## 🔢 Comprendre Space.Self vs Space.World

### `Space.Self` (utilisé dans le code) :
- Rotation dans l'**espace local** de l'objet
- Les axes tournent **avec l'objet**
- ✅ **Recommandé** pour des rotations relatives au prefab

### `Space.World` :
- Rotation dans l'**espace global** de la scène
- Les axes restent fixes (X=droite, Y=haut, Z=avant)
- Utilisé pour des rotations absolues

## 🧪 Test et vérification

### Dans l'application Android :

1. **Buildez** l'application (File → Build Settings → Build And Run)
2. **Lancez** l'application
3. **Détectez** le marker AR
4. **Observez** : Le prefab Temperature devrait être incliné de -30.808° sur X
5. **Bouton NEXT** : Vérifiez que GT4 et A8 ne sont PAS affectés

### Dans la Console Unity :

Lors de l'instanciation, vous devriez voir :
```
[SPAWN] Prefab 'Temperature(Clone)' créé pour ImageName (Index: 0)
[SPAWN] Rotation X -30.808° appliquée au prefab Temperature
```

## 📊 Alternatives

### Option 1 : Rotation dans le script (ACTUEL) ✅
- ✅ Facile à ajuster en temps réel
- ✅ Différente pour chaque prefab
- ❌ Nécessite rebuild pour chaque changement

### Option 2 : Rotation dans le prefab
- ✅ Modifiable dans Unity sans rebuild
- ✅ Visible immédiatement dans l'éditeur
- ❌ Plus complexe si rotation conditionnelle

### Option 3 : Les deux combinés
- Rotation de base dans le prefab
- Ajustement fin dans le script
- ✅ Maximum de flexibilité

## 💡 Recommandation

Si la rotation **-30.808°** est **constante** pour Temperature :
→ Mieux vaut la mettre **directement dans le prefab** (ROOT → Transform → Rotation X)

Si la rotation est **variable** selon le contexte :
→ Gardez la dans le **script** (comme actuellement) ✅

## ✅ Checklist de vérification

- [ ] Le script `DynamicTrackedImageHandler.cs` est modifié
- [ ] La rotation s'applique uniquement à Temperature
- [ ] L'application est rebuildée
- [ ] Test sur Android : Temperature a la bonne orientation
- [ ] Test avec NEXT : GT4 et A8 ne sont pas affectés

---

**Fichier modifié** : `Assets/Scripts/DynamicTrackedImageHandler.cs`  
**Ligne** : 122-129  
**Rotation appliquée** : -30.808° sur l'axe X pour Temperature uniquement
