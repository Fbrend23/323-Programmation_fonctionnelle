# Opérateur de puissance... en mode récursif

## Histoire : Les Origines du Calcul de Puissance

Au 9ᵉ siècle, le mathématicien perse **Al-Khwārizmī** (dont le nom a donné le mot "algorithme") travaillait sur des méthodes pour calculer rapidement des multiplications répétées. Il cherchait une façon élégante de calculer 2⁸ sans faire 8 multiplications à la suite.

Son intuition ? **Décomposer le problème** :
- Si je sais calculer 2⁷, alors 2⁸ = 2 × 2⁷
- Si je sais calculer 2⁶, alors 2⁷ = 2 × 2⁶
- ...et ainsi de suite jusqu'à 2¹ = 2

Cette idée simple est à la base de la **récursivité** en programmation !

## Les Mathématiques Derrière

### Définition mathématique de la puissance

Pour tout nombre réel `x` et tout entier naturel `n` :

```
x⁰ = 1                    (cas de base)
xⁿ = x × xⁿ⁻¹             (cas récursif)
```

### Exemples

- 3⁴ = 3 × 3³ = 3 × (3 × 3²) = 3 × (3 × (3 × 3¹)) = 3 × 3 × 3 × 3 = 81
- 5⁰ = 1 (par convention mathématique)
- 2⁵ = 2 × 2⁴ = 2 × 16 = 32

---

## Exercice : Implémenter la Fonction Puissance

### Objectif
Créer une fonction récursive `Puissance` en C# qui calcule xⁿ.

### Signature de la fonction
```csharp
public static double Power(double baseNumber, int exponent)
{
    // À compléter
}
```

**Contrainte :**
- La fonction **DOIT être récursive** (elle s'appelle elle-même)
- Ne pas utiliser de boucles (`for`, `while`, etc.)
- Ne pas utiliser `Math.Pow()`

### Exemples de tests

```csharp
Console.WriteLine(Power(2, 3));    // Attendu : 8
Console.WriteLine(Power(5, 0));    // Attendu : 1
Console.WriteLine(Power(3, 4));    // Attendu : 81
Console.WriteLine(Power(10, 2));   // Attendu : 100
Console.WriteLine(Power(2, 10));   // Attendu : 1024
```

---

## Indices (à lire progressivement)

<details>
<summary>Indice 1 : Structure générale</summary>

Toute fonction récursive a deux parties :
1. **Le cas de base** : quand arrêter la récursion ?
2. **Le cas récursif** : comment décomposer le problème ?

</details>

<details>
<summary>Indice 2 : Le cas de base</summary>

Quelle est la seule puissance qu'on connaît directement sans calcul ?
Indice : x⁰ = ?

</details>

<details>
<summary>Indice 3 : Le cas récursif</summary>

Si `n > 0`, comment exprimer xⁿ en fonction de xⁿ⁻¹ ?
Rappel : xⁿ = x × xⁿ⁻¹

</details>

## Questions de Réflexion

1. Combien d'appels récursifs sont nécessaires pour calculer 2⁵ ?
2. Que se passe-t-il si on oublie le cas de base ? (essayer)
3. Pourquoi x⁰ = 1 pour tout x (sauf 0) ?
4. Comment pourrait-on optimiser cette fonction pour les grands exposants ?

## Pour Aller Plus Loin (Bonus)

Une fois votre fonction terminée :
- Comparer les performances avec `Math.Pow()`
- Réfléchir : peut-on calculer les puissances négatives ? (ex: 2⁻³ = 1/8)
