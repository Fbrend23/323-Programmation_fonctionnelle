# Fibonacci

## Un peu d'histoire

Leonardo Fibonacci, mathématicien italien du 13ème siècle, a introduit cette suite en étudiant la reproduction des lapins. Chaque nombre est la somme des deux précédents : **0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89...**

Cette suite est aujourd'hui utilisée en **Planning Poker Agile** pour estimer la complexité des tâches : les développeurs votent avec des cartes portant ces nombres, reflétant l'incertitude croissante des estimations.


## Définition mathématique

La suite de Fibonacci est une suite d'entiers définie par récurrence. Elle possède deux cas de base et une relation de récurrence.

### Cas de base

Les deux premiers termes de la suite sont définis explicitement :
```
F(0) = 0
F(1) = 1
```

Ces valeurs initiales sont essentielles car elles servent de point de départ pour calculer tous les autres termes.

### Relation de récurrence

Pour tout entier naturel n supérieur ou égal à 2, chaque terme est défini comme la somme des deux termes précédents :

```
F(n) = F(n-1) + F(n-2)  pour n ≥ 2
```

### Exemples de calcul

```
F(2) = F(1) + F(0) = 1 + 0 = 1
F(3) = F(2) + F(1) = 1 + 1 = 2
F(4) = F(3) + F(2) = 2 + 1 = 3
F(5) = F(4) + F(3) = 3 + 2 = 5
F(6) = F(5) + F(4) = 5 + 3 = 8
```

## Objectif

Implémenter la suite de Fibonacci de manière **récursive** en C#.

## Consignes par étapes

### Étape 1 : Analyser la structure récursive

Avant de coder, identifiez :
- Quels sont les **cas de base** qui arrêtent la récursion ?
- Quel est le **cas récursif** qui décompose le problème ?
- Quelle est la **condition de terminaison** ?

### Étape 2 : Créer la méthode récursive

- Créer une méthode `static int Fibonacci(int n)`
- Implémenter les **cas de base** (n = 0 et n = 1)
- Implémenter le **cas récursif** en appliquant la formule F(n) = F(n-1) + F(n-2)

### Étape 3 : Tester la fonction

Dans  `Main()`, afficher les 13 premiers nombres de Fibonacci correspondant aux cartes du Planning Poker :

```
0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144
```

### Étape 4 : Validation et gestion d'erreurs

Ajouter une protection contre les valeurs négatives de `n`.

### Étape 5 : Version alternative

L’implémentation récursive suggérée avec un seul paramètre a un défaut majeur : elle recalcule les mêmes valeurs de nombreuses fois. Par exemple, pour calculer `F(5)` :

```
F(5)
├── F(4)
│   ├── F(3)
│   │   ├── F(2)
│   │   │   ├── F(1)
│   │   │   └── F(0)
│   │   └── F(1)
│   └── F(2)
│       ├── F(1)
│       └── F(0)
└── F(3)
    ├── F(2)
    │   ├── F(1)
    │   └── F(0)
    └── F(1)
```

On remarque que `F(3)` est calculé 2 fois, `F(2)` est calculé 3 fois, et `F(1)` est calculé 5 fois !


#### La solution : récursivité terminale avec accumulateurs

Au lieu de recalculer les valeurs, on peut **transporter** les deux valeurs précédentes à chaque appel récursif :

```
FibonacciOptimise(iteration, precedent, actuel)
```

**Principe** :
- On initialise avec `precedent = 0` et `actuel = 1` (les deux premiers termes)
- À chaque appel récursif, on "avance" d'un cran : le nouveau précédent devient l'actuel, et le nouveau actuel devient la somme
- On décrémente iteration jusqu'à atteindre 0

#### Mission

Implémenter une méthode `FibonacciOptimise(int iteration, int precedent = 0, int actuel = 1)` qui utilise cette approche.

**Indices** :
1. Les cas de base changent : que retourner si `iteration == 0` ? Et si `iteration == 1` ?
2. Pour le cas récursif, appeler la fonction avec :
   - `iteration - 1`
   - `actuel` (devient le nouveau précédent)
   - `precedent + actuel` (devient le nouveau actuel)

### Test de performance

Comparer les deux versions :
- Mesurer le temps pour calculer `Fibonacci(35)` avec la version naïve
- Mesurer le temps pour calculer `Fibonacci(35)` avec la version optimisée
- Essayer `FibonacciOptimise(10000)`

> Utiliser `System.Diagnostics.Stopwatch` pour mesurer le temps d'exécution.

## Activités bonus
- Implémenter une version **itérative** de Fibonacci et comparer les performances avec un ‘StopWatch’
