using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Suite de Fibonacci (version récursive simple) :");

        // Étape 3 : afficher les 13 premiers nombres utilisés en Planning Poker
        // 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144
        for (int i = 0; i <= 12; i++)
        {
            int value = Fibonacci(i);
            Console.Write(value);

            if (i < 12)
            {
                Console.Write(", ");
            }
        }

        Console.WriteLine();
        Console.WriteLine();

        // Démonstration de la version optimisée
        Console.WriteLine("Fibonacci optimisé (récursivité terminale) :");
        for (int i = 0; i <= 12; i++)
        {
            int value = FibonacciOptimise(i);
            Console.Write(value);

            if (i < 12)
            {
                Console.Write(", ");
            }
        }

        Console.WriteLine();
        Console.WriteLine();

        // Démonstration de la version itérative
        Console.WriteLine("Fibonacci (version itérative) :");
        for (int i = 0; i <= 12; i++)
        {
            int value = FibonacciIteratif(i);
            Console.Write(value);

            if (i < 12)
            {
                Console.Write(", ");
            }
        }

        Console.WriteLine();
        Console.WriteLine();

        // Test de performance
        // Objectif : montrer que la version naïve est lente quand n devient grand
        var sw = new Stopwatch();

        // Version naïve
        sw.Start();
        int fib35_naif = Fibonacci(35);
        sw.Stop();
        Console.WriteLine($"Fibonacci(35) naif = {fib35_naif} en {sw.ElapsedTicks} ticks ({sw.ElapsedMilliseconds} ms)");

        // Version optimisée
        sw.Restart();
        int fib35_opt = FibonacciOptimise(35);
        sw.Stop();
        Console.WriteLine($"FibonacciOptimise(35) = {fib35_opt} en {sw.ElapsedTicks} ticks ({sw.ElapsedMilliseconds} ms)");

        // Version itérative
        sw.Restart();
        int fib35_iter = FibonacciIteratif(35);
        sw.Stop();
        Console.WriteLine($"FibonacciIteratif(35) = {fib35_iter} en {sw.ElapsedTicks} ticks ({sw.ElapsedMilliseconds} ms)");

        Console.WriteLine();

        // Test d'un très grand n avec la version optimisée
        // Attention : ici on reste en int, donc on ne peut pas aller très loin sans déborder.
        // On fait juste la démo avec 10 000 pour montrer que la fonction termine.
        sw.Restart();
        int fib10000 = FibonacciOptimise(10000);  // ne l'affiche pas, il ne tient pas dans un int
        sw.Stop();
        Console.WriteLine($"FibonacciOptimise(10000) calculé en {sw.ElapsedTicks} ticks ({sw.ElapsedMilliseconds} ms)");
    }

    // ---------------------------------------------------------
    // Étape 2 : méthode récursive de base
    // ---------------------------------------------------------
    static int Fibonacci(int n)
    {
        // Étape 4 : protection contre les valeurs négatives
        if (n < 0)
        {
            throw new ArgumentException("n ne peut pas être négatif.");
        }

        // Cas de base
        if (n == 0)
        {
            return 0;
        }

        if (n == 1)
        {
            return 1;
        }

        // Cas récursif : F(n) = F(n-1) + F(n-2)
        return Fibonacci(n - 1) + Fibonacci(n - 2);
    }

    // ---------------------------------------------------------
    // Étape 5 : version récursive optimisée (récursivité terminale)
    // Idée : on transporte les deux derniers termes au lieu de les recalculer
    // ---------------------------------------------------------
    static int FibonacciOptimise(int iteration, int precedent = 0, int actuel = 1)
    {
        if (iteration < 0)
        {
            throw new ArgumentException("iteration ne peut pas être négatif.");
        }

        // Nouveau cas de base 1 :
        // si on demande la valeur à l'itération 0, on retourne le précédent
        // Ex. FibonacciOptimise(0) -> 0
        if (iteration == 0)
        {
            return precedent;
        }

        // Nouveau cas de base 2 :
        // si on demande la valeur à l'itération 1, on retourne l'actuel
        // Ex. FibonacciOptimise(1) -> 1
        if (iteration == 1)
        {
            return actuel;
        }

        // Cas récursif :
        // on avance d'un cran : (iteration - 1)
        // le nouveau précédent devient l'actuel
        // le nouveau actuel devient la somme des deux précédents
        return FibonacciOptimise(iteration - 1, actuel, precedent + actuel);
    }

    // ---------------------------------------------------------
    // Activité bonus : version itérative
    // Cette version est souvent la plus simple et la plus rapide
    // ---------------------------------------------------------
    static int FibonacciIteratif(int n)
    {
        if (n < 0)
        {
            throw new ArgumentException("n ne peut pas être négatif.");
        }

        if (n == 0)
        {
            return 0;
        }

        if (n == 1)
        {
            return 1;
        }

        int precedent = 0;
        int actuel = 1;

        for (int i = 2; i <= n; i++)
        {
            int suivant = precedent + actuel;
            precedent = actuel;
            actuel = suivant;
        }

        return actuel;
    }
}
