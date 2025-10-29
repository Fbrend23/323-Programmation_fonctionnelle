# Listing Récursif de Fichiers

## Un peu d'histoire et de contexte

Les systèmes de fichiers hiérarchiques ont été introduits dans les années 1960 avec UNIX. Cette structure en arbre permet d'organiser les données de manière logique : un dossier peut contenir des fichiers et d'autres dossiers, créant ainsi une structure récursive naturelle.

Cette organisation est fondamentale dans tous les systèmes d'exploitation modernes (Windows, Linux, macOS) et correspond parfaitement à un problème résolu par récursivité.

En pratique, de nombreux outils utilisent ce principe : les commandes `ls -R` (Linux), `tree` (Windows/Linux), les indexeurs de fichiers, les moteurs de recherche de bureau, et même les outils de sauvegarde qui doivent parcourir l'ensemble d'une arborescence.

---

## Définition algorithmique

Le parcours récursif d'une arborescence de fichiers est un exemple classique de **parcours en profondeur** (Depth-First Search) d'une structure arborescente.

### Structure de données

Un système de fichiers peut être modélisé comme un arbre où :
- Les **feuilles** sont les fichiers
- Les **noeuds internes** sont les dossiers
- Chaque noeud peut avoir 0 ou plusieurs enfants

### Cas de base

Le cas de base se produit lorsqu'on atteint un **fichier** :
```
Si l'élément est un fichier :
    Afficher le nom du fichier
    Terminer
```

### Cas récursif

Le cas récursif se produit lorsqu'on atteint un **dossier** :
```
Si l'élément est un dossier :
    Afficher le nom du dossier
    Pour chaque élément dans le dossier :
        Appeler récursivement la fonction sur cet élément
```

### Algorithme général

```
Fonction ListeFichiers(chemin, niveau_indentation)
    Si chemin est un fichier :
        Afficher indentation + nom du fichier
    Sinon si chemin est un dossier :
        Afficher indentation + nom du dossier
        Pour chaque élément dans le dossier :
            ListeFichiers(élément, niveau_indentation + 1)
```

### Exemple d'arborescence

Considérons la structure suivante :
```
Projet/
├── src/
│   ├── Program.cs
│   └── Utils.cs
├── tests/
│   └── TestUtils.cs
└── README.md
```

Le parcours récursif explorera :
1. Projet/ (dossier)
2. src/ (dossier)
3. Program.cs (fichier - cas de base)
4. Utils.cs (fichier - cas de base)
5. tests/ (dossier)
6. TestUtils.cs (fichier - cas de base)
7. README.md (fichier - cas de base)

### Propriétés importantes

- **Profondeur** : La profondeur maximale correspond au nombre de niveaux de dossiers imbriqués
- **Ordre de parcours** : L'ordre dépend de l'implémentation (alphabétique, ordre du système de fichiers, etc.)
- **Terminaison** : Garantie par le fait qu'un système de fichiers sain ne contient pas de cycles

## Objectif

Implémenter une fonction récursive en C# qui liste tous les fichiers et dossiers à partir d'un répertoire donné.

---

## Consignes par étapes

### Étape 1 : Comprendre les classes du framework

En C#, vous aurez besoin des classes suivantes du namespace `System.IO` :
- `Directory.Exists(chemin)` : vérifie si un dossier existe
- `File.Exists(chemin)` : vérifie si un fichier existe
- `Directory.GetFiles(chemin)` : retourne un tableau de chemins de fichiers
- `Directory.GetDirectories(chemin)` : retourne un tableau de chemins de sous-dossiers
- `Path.GetFileName(chemin)` : extrait le nom d'un fichier ou dossier depuis son chemin complet

### Étape 2 : Créer la méthode récursive de base

- Créez une méthode `static void ListerFichiers(string chemin)`
- Implémentez le **cas de base** : si c'est un fichier, affichez son nom
- Implémentez le **cas récursif** : si c'est un dossier, listez son contenu puis appelez récursivement sur chaque élément

### Étape 3 : Améliorer l'affichage avec l'indentation

- Ajouter un paramètre `int niveau` à votre méthode : `ListerFichiers(string chemin, int niveau = 0)`
- Utiliser ce niveau pour créer une indentation visuelle (par exemple : `new string(' ', niveau * 2)`)
- Différenciez visuellement les dossiers des fichiers (par exemple : `[D]` pour dossier, `[F]` pour fichier)

### Étape 4 : Gestion des erreurs

Ajouter une gestion des exceptions car certains dossiers peuvent être inaccessibles (permissions insuffisantes) :
- Utiliser un bloc `try-catch` pour capturer les `UnauthorizedAccessException`
- Afficher un message approprié si l'accès est refusé

### Étape 5 : Tester la fonction

Dans votre `Main()` :
- Créer un dossier de test avec quelques sous-dossiers et fichiers
- Appeler la fonction sur ce dossier
- Tester également sur un dossier système (par exemple : le dossier temporaire)

## Questions de réflexion

1. **Condition de terminaison** : Qu'est-ce qui garantit que votre fonction récursive s'arrêtera toujours ? Que se passerait-il en présence de liens symboliques circulaires ?

2. **Ordre de parcours** : Dans quel ordre les fichiers et dossiers sont-ils visités ? Comment  modifier cet ordre (par exemple, pour trier alphabétiquement) ?

3. **Comparaison itératif/récursif** : Implémenter la même fonctionnalité de manière itérative avec une pile (Stack) ou une liste... ? Quels seraient les avantages et inconvénients ?

5. **Cas limites** : Que se passe-t-il si :
   - Le chemin fourni n'existe pas ?
   - Le dossier est vide ?
   - La profondeur est très importante (ex: 1000 niveaux) ?

6. **Extension pratique** : Comment modifier la fonction pour :
   - Ne lister que les fichiers d'une certaine extension (.cs, .txt) ?
   - Calculer la taille totale de tous les fichiers ?
   - Compter le nombre de fichiers et de dossiers ?

## Pour aller plus loin (optionnel)

- Ajoutez un paramètre pour limiter la profondeur maximale de récursion
- Implémentez un filtre avec des expressions régulières pour ne lister que certains fichiers
- Calculez et affichez la taille de chaque fichier
- Créez une version qui retourne une structure de données (arbre) plutôt que d'afficher directement
- Implémentez un système de statistiques (nombre total de fichiers, taille totale, fichier le plus volumineux, etc.)
- Comparez les performances entre `Directory.GetFileSystemEntries()` et l'approche avec `GetFiles()` + `GetDirectories()`

## Exemple de résultat

```
[D] MonProjet
  [D] src
    [F] Program.cs
    [F] Utils.cs
  [D] tests
    [F] TestProgram.cs
  [F] README.md
  [F] MonProjet.csproj
```
