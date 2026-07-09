Vous allez faire le claude.md d'un projet sur lequel je travaille.

Il s'agit d'une logiciel Windows C# console ou windows forms pour faire de la musique en utilisant une manette de jeu vidéo comme input.
Le logicel permettra de jouer des accords de musique et / ou des gammes.

Je vais tester le logiciel avec ma manette d'arcade Mayflash F500 Elite.

On pourra dans le logiciel configuer le laytout. Il y aura des presets prédéfinis dans un fichier de configuration lisible, quelque chose comme du json

On pourra ajouter des presets dans le fichier et choisir son preset.

L'engin audio devra si possible se comporter comme un SoundPool pour android.

J'ai déjà fait une application Android du genre et pour la partie "output audio", j'ai déjà commencé à migrer le code java vers C#.

Le principe d'input sera le suivant:

avec les boutons de droite (8 boutons) on joue les notes de la gamme ou l'accord

position neutre du joystick: accord ou gamme principale, par exemple, E majeur.

Si on bouge le joystick, ça change l'accord.

L'engin sera polyphonique au nombre de boutons (boutons à droite) de la manette)

Si on bouge le joystick pendant que ça joue, ça va changer l'accord ou la gamme et si un bouton est déjà pressé, il se fera changé de pitch
soit remplacé par un autre sample soit pitch-bendé.

Path du projet Java de référence pour l'engin audio: ..\..\Java\VirtuoPhone\src\com\virtuophone

Si on appuie rapidement à gauche ou à droite ou en haut ou en bas 2 fois de suite (un peu comme pour courrir dans Illusion of Gaia au snes,
comme un double tap, ça permettra de choisir un autre preset que celui de la position choisie.

Exemples de presets, le 1er utilise le double tap:



Avec double tap sur les extrémités
          |B+|
       |D |F#|G |
|F#dim7|F |E |B |C#dim7|
       |C |Bb|A |
          |E+|



Chromatic Spiral version
|C |D |F |
|B |E |F#|
|Bb|A |G |


Chromatic Spiral version circle of 5th
|F#|B |A |
|Bb|E |D |
|F |C |G |


Minor rotation
|C |D |F |
|B |E |F#|
|Bb|A |G |


Minor blues rotation tensions on sides, stable on diagonals
|B |C |D |
|Bb|E |F#|
|A |F |G |


Minor blues rotation tensions on diagonals, stable on sides
|C |D |F#|
|B |E |G |
|Bb|A |F |

F# B E A D G C F Bb




Pure circle of 5th rotation
|G#|C#|F#|
|D#|E |B |
|A#|F |C |


Pure circle of 4th rotation
|C |G |D |
|F |E |A |
|Bb|Eb|Ab|


Half circle of 4th / 5th rotation
|G |D |A |
|C |E |B |
|G#|C#|F#|


Max distance diagonal neutral root in diagonal (bottom left)
|F#|F |Bb|
|B |G |C |
|E |A |D |


Center dim7 most instable, sides instable, corners stable
|C |Bb|D#|
|G |Bo|C#|
|A |E |F#|


Il y aura aussi la possibilité de moduler. Si on sélectionne une position et on appuie "start", ça va faire en sorte que la position neutre aura cette tonalité.
Le preset sera donc transposé, exemple, le E du centre pourrait devenir un A, et le reste serait relatif.
Si on appuie rapidement sur start 2 fois, au lieu d'une fois, alors la position du centre sera mineure (par exemple, accord mineur), mais le reste sera tel quel donc si non spécifié dans le preset: majeur,
si spécifié dans le preset, tel que spécifié.

Le bouton Select permettra de changer d'instrument.

Il y aura aussi la possiblité de remapper les contrôles de la manette pour chaque action.

Avant de faire le claude.md, on va s'assurer que vous comprenez le projet. Vous ferez le claude.md quand je vous le dirai.