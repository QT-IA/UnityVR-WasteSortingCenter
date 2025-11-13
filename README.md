"# Unity VR - Centre de Tri des Déchets

Projet Unity VR de simulation d'un centre de tri des déchets avec système de spawn aléatoire et détection automatique.

## Objectifs du Projet

1. Mettre en place une scène 3D avec objets, colliders, matériaux et scripts
2. Système de spawn infini et aléatoire des déchets
3. Destruction automatique des déchets à l'extrémité du tapis roulant
4. Détection des déchets inappropriés avec feedback visuel (à venir)

## Scripts Disponibles

### 1. `Spawner.cs` - Spawn Aléatoire Infini

**Fonction :** Génère continuellement des déchets (Carton0, Carton1, Bottle) selon des probabilités configurables.

**Configuration :**

- **Prefabs** : Assignez les 3 prefabs dans l'Inspector
  - `Carton0` : Premier type de carton
  - `Carton1` : Deuxième type de carton
  - `Bottle` : Bouteille
- **Probabilités** : Poids relatifs (normalisés automatiquement)
  - `weightCarton0` : 0-1 (défaut: 0.33)
  - `weightCarton1` : 0-1 (défaut: 0.33)
  - `weightBottle` : 0-1 (défaut: 0.34)
- **Spawn Settings** :
  - `spawnPoints[]` : Liste de Transform où spawner (optionnel)
  - `spawnInterval` : Délai entre chaque spawn en secondes (défaut: 2s)
  - `loop` : Active/désactive le spawn automatique

**Utilisation :**

```csharp
// Spawn manuel depuis un autre script
var spawner = GetComponent<Spawner>();
spawner.SpawnOne();
```

### 2. `WasteDestroyer.cs` - Destruction Automatique

**Fonction :** Détruit les déchets qui entrent dans un collider Trigger.

**Configuration :**

- `targetTag` : Tag des objets à détruire (vide = tout détruire)
- `debugLog` : Active les logs de debug
- `destroyDelay` : Délai avant destruction en secondes (0 = immédiat)

**Setup dans Unity :**

1. Créer un GameObject à l'extrémité du tapis (ex: "ConveyorEnd")
2. Ajouter un Collider (Box/Sphere) et cocher **"Is Trigger"**
3. Attacher le script `WasteDestroyer`
4. Ajuster la taille du collider pour couvrir la zone de sortie

## Installation et Configuration

### Prérequis

- Unity 2021.3 ou supérieur
- Packages installés :
  - XR Interaction Toolkit
  - Universal Render Pipeline (URP)

### Étapes de Configuration

#### 1. Configuration du Spawner

```
1. Créer un GameObject vide : "WasteSpawner"
2. Attacher le script Spawner.cs
3. Dans l'Inspector :
   - Assigner les prefabs Carton0, Carton1, Bottle
   - Créer des Empty GameObjects comme points de spawn
   - Les ajouter au tableau spawnPoints[]
   - Régler spawnInterval (ex: 2 secondes)
   - Cocher "Loop"
```

#### 2. Configuration du Destroyer

```
1. Créer un GameObject : "ConveyorEndZone"
2. Positionner à l'extrémité du tapis roulant
3. Ajouter un Box Collider
   - Cocher "Is Trigger"
   - Ajuster la taille pour couvrir la zone
4. Attacher WasteDestroyer.cs
```

#### 3. Configuration des Prefabs (Recommandé)

Pour chaque prefab de déchet :

```
1. Ajouter un Rigidbody
   - Masse : 0.5 - 2 kg
   - Use Gravity : activé
   - Constraints : selon besoins (ex: freeze rotation Z/X)

2. Ajouter un Collider adapté
   - Box Collider pour cartons
   - Capsule/Mesh Collider pour bottles

3. (Optionnel) Ajouter un tag "Waste"
```

## Structure du Projet

```
TrashProject/
├── Assets/
│   ├── Prefabs/                    # Prefabs des déchets
│   ├── Scenes/                     # Scènes Unity
│   ├── WasteSortingCenterPack/
│   │   └── Scripts/
│   │       ├── Spawner.cs          # Spawn aléatoire infini
│   │       └── WasteDestroyer.cs   # Destruction automatique
│   ├── XR/                         # Configuration VR
│   └── XRI/                        # XR Interaction Toolkit
├── ProjectSettings/                # Paramètres Unity
└── README.md                       # Ce fichier
```

## Fonctionnement du Système

### Cycle de Vie d'un Déchet

```
1. [Spawner] Génère un déchet aléatoire
   └─> Choix selon probabilités (weightCarton0/1/Bottle)
   └─> Instanciation au spawnPoint (ou position du Spawner)

2. [Physique Unity] Le déchet tombe/bouge sur le tapis
   └─> Rigidbody + gravité
   └─> Peut être déplacé par le joueur (XR Grab)

3. [WasteDestroyer] Détection à l'extrémité
   └─> OnTriggerEnter détecte l'entrée
   └─> Destroy() du GameObject

4. [Spawner] Génère le prochain déchet après spawnInterval
```

## Paramètres Recommandés

### Pour un gameplay équilibré

| Paramètre        | Valeur Recommandée | Description              |
| ----------------- | ------------------- | ------------------------ |
| `spawnInterval` | 2-3 secondes        | Temps entre chaque spawn |
| `weightCarton0` | 0.4                 | 40% de cartons type 0    |
| `weightCarton1` | 0.4                 | 40% de cartons type 1    |
| `weightBottle`  | 0.2                 | 20% de bouteilles        |
| `destroyDelay`  | 0 secondes          | Destruction immédiate   |

### Pour un gameplay difficile

| Paramètre        | Valeur         | Description           |
| ----------------- | -------------- | --------------------- |
| `spawnInterval` | 1-1.5 secondes | Spawn plus rapide     |
| Poids égaux      | 0.33/0.33/0.34 | Distribution uniforme |

## Debug et Troubleshooting

### Problème : Les déchets ne spawn pas

**Solutions :**

- Vérifier que les prefabs sont assignés dans l'Inspector
- Vérifier que `loop` est coché
- Vérifier que `spawnInterval > 0`
- Regarder la console Unity pour les warnings

### Problème : Les déchets ne sont pas détruits

**Solutions :**

- Vérifier que le Collider du WasteDestroyer est en mode **Trigger**
- Vérifier que le tag correspond (si targetTag est renseigné)
- Activer `debugLog` pour voir les détections
- Vérifier la taille du collider (peut être trop petit)

### Problème : Les déchets traversent le sol

**Solutions :**

- Ajouter un Collider au sol (pas en Trigger)
- Vérifier que les prefabs ont un Rigidbody
- Vérifier les layers et la Collision Matrix (Edit > Project Settings > Physics)

## TODO / Améliorations Futures

- [ ] Ajouter `WasteItem.cs` pour identifier chaque type de déchet
- [ ] Ajouter `ConveyorEnd.cs` pour détecter les déchets inappropriés
- [ ] Ajouter `FeedbackManager.cs` pour feedback visuel (lumière rouge, particules)
- [ ] Implémenter un système d'Object Pooling (performance)
- [ ] Ajouter un compteur de score
- [ ] Ajouter des sons (spawn, destruction, erreur)
- [ ] Ajouter un UI pour afficher les statistiques
- [ ] Implémenter un système de progression (niveaux de difficulté)

## Ressources et Documentation

- [Unity XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.0/manual/index.html)
- [Unity Physics - Triggers](https://docs.unity3d.com/Manual/CollidersOverview.html)
- [Unity Coroutines](https://docs.unity3d.com/Manual/Coroutines.html)

## Auteurs

Projet réalisé dans le cadre d'un exercice Unity VR.

## Licence

Ce projet est à usage éducatif.

---

**Version :** 1.0
**Dernière mise à jour :** Novembre 2025
**Moteur :** Unity 2021.3+"
