# TP RobotPaint

## Principe du jeu

1. **Phase 1 : Dessin.** Vue du dessus : on peint la carte à la souris sur une texture de 32×32 pixels (clic gauche pour peindre, clic droit pour gommer).
2. **Validation.** Chaque pixel devient une case :
   | Couleur | Case |
   |---|---|
   | Blanc | sol libre |
   | Noir | mur |
   | Bleu | départ du robot (A) |
   | Magenta | arrivée du robot (B) |
   Le NavMesh est ensuite généré automatiquement.
3. **Phase 2 : Jeu.** Le robot part de A et va jusqu'à B.

## Lancer le projet

Ouvrir la scène `Assets/Scenes/RobotPaint.unity`, puis Play.

## Organisation des scripts

### `Scripts/TP/` : les scripts à modifier

| Script | Rôle |
|---|---|
| `MapBuilder` | Transforme le dessin en carte 3D : une couleur = un prefab. |
| `ScoreManager` | Score de la partie (à transformer en singleton). |
| `ScoreZone` | Case Jaune : donne des points (à écrire). |
| `DeathZone` / `SpeedBoostZone` | Cases Rouge / Verte : exemples de zones déjà écrits. |
| `MapSaveSystem` | Sauvegarde et chargement des cartes. |

### `Scripts/Core/` : le moteur du TP (pas besoin de le modifier)

Ce dossier contient la peinture (`MapPainter`), le robot (`RobotController`), les phases de jeu (`GameManager`) et l'interface (`UIManager`, `ColorButton`).

Cherchez `TODO` dans le dossier `TP` pour trouver les points à compléter.

---

## Étape 1 : Nouvelles couleurs

| Couleur | Effet | Prefab |
|---|---|---|
| Jaune | objectif de score | `Prefabs/ScoreZone` |
| Rouge | mort | `Prefabs/DeathZone` |
| Vert | boost de vitesse | `Prefabs/SpeedBoostZone` |

1. `MapBuilder` : ajouter les 3 prefabs en variables, puis un `else if` par couleur dans `Build`.
2. Dans l'inspecteur de l'objet `MapBuilder`, assigner les 3 prefabs.
3. UI : dans `Canvas/PaintPanel/Palette`, dupliquer un bouton, puis changer `Color` (composant `ColorButton`) et le texte du bouton.

> ⚠️ Le bouton et `MapBuilder` doivent utiliser **exactement** la même couleur. `Color.yellow` de Unity n'est pas un jaune pur : utilisez `new Color(1, 1, 0)`.

### Le score

`DeathZone` et `SpeedBoostZone` sont déjà écrits. Il reste le score à faire :

1. `ScoreManager` : le transformer en **singleton**, pour qu'on puisse l'appeler depuis n'importe quel script avec `ScoreManager.Instance`.
2. `ScoreZone` : écrire `OnTriggerEnter`. Si c'est le robot qui entre, on ajoute les points au `ScoreManager`, puis on détruit la zone. Inspirez-vous de `DeathZone`.

## Étape 2 : Sauvegarde / chargement

Il faut compléter les 4 fonctions de `MapSaveSystem`. Les boutons de l'UI les appellent déjà.

| Fonction | Rôle |
|---|---|
| `SaveMap(nom)` | Écrire la carte sur le disque. |
| `GetAvailableSaves()` | Lister les sauvegardes présentes. |
| `RefreshSaveList()` | Afficher la liste dans l'UI. Appelée automatiquement au lancement. |
| `OnLoad(nom)` | Charger la carte choisie dans la liste. |

Format conseillé : **PNG** (`EncodeToPNG` / `LoadImage`), puisque la carte est déjà une image.
