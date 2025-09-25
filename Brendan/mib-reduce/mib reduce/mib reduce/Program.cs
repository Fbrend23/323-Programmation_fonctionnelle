using System;
using System.Collections.Generic;
using System.Linq;

// Modèle minimal
public class Product
{
    public int Location { get; set; }
    public string Producer { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public string Unit { get; set; } = "";
    public double PricePerUnit { get; set; }
}

class Program
{
    // Rendre la liste statique (visible depuis Main) et NE PAS la réécrire dans Main
    static readonly List<Product> products = new()
    {
 new Product { Location = 1, Producer = "Bornand", ProductName = "Pommes", Quantity = 20,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 1, Producer = "Bornand", ProductName = "Poires", Quantity = 16,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 1, Producer = "Bornand", ProductName = "Pastèques", Quantity = 14,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 1, Producer = "Bornand", ProductName = "Melons", Quantity = 5,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 2, Producer = "Dumont", ProductName = "Noix", Quantity = 20,Unit = "sac", PricePerUnit = 5.50 },
    new Product { Location = 2, Producer = "Dumont", ProductName = "Raisin", Quantity = 6,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 2, Producer = "Dumont", ProductName = "Pruneaux", Quantity = 13,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 2, Producer = "Dumont", ProductName = "Myrtilles", Quantity = 12,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 2, Producer = "Dumont", ProductName = "Groseilles", Quantity = 12,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 3, Producer = "Vonlanthen", ProductName = "Pêches", Quantity = 8,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 3, Producer = "Vonlanthen", ProductName = "Haricots", Quantity = 6,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 3, Producer = "Vonlanthen", ProductName = "Courges", Quantity = 18,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 3, Producer = "Vonlanthen", ProductName = "Tomates", Quantity = 12,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 3, Producer = "Vonlanthen", ProductName = "Pommes", Quantity = 20,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 4, Producer = "Barizzi", ProductName = "Poires", Quantity = 5,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 4, Producer = "Barizzi", ProductName = "Pastèques", Quantity = 6,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 4, Producer = "Barizzi", ProductName = "Melons", Quantity = 14,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 4, Producer = "Barizzi", ProductName = "Noix", Quantity = 20,Unit = "sac", PricePerUnit = 5.50 },
    new Product { Location = 4, Producer = "Barizzi", ProductName = "Raisin", Quantity = 15,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 5, Producer = "Blanc", ProductName = "Pruneaux", Quantity = 5,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 5, Producer = "Blanc", ProductName = "Myrtilles", Quantity = 18,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 5, Producer = "Blanc", ProductName = "Groseilles", Quantity = 10,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 5, Producer = "Blanc", ProductName = "Pêches", Quantity = 20,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 5, Producer = "Blanc", ProductName = "Haricots", Quantity = 9,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 6, Producer = "Repond", ProductName = "Courges", Quantity = 12,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 6, Producer = "Repond", ProductName = "Tomates", Quantity = 12,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 6, Producer = "Repond", ProductName = "Pommes", Quantity = 15,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 6, Producer = "Repond", ProductName = "Poires", Quantity = 18,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 6, Producer = "Repond", ProductName = "Pastèques", Quantity = 7,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 7, Producer = "Mancini", ProductName = "Pêches", Quantity = 10,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 7, Producer = "Mancini", ProductName = "Haricots", Quantity = 11,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 7, Producer = "Mancini", ProductName = "Courges", Quantity = 10,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 7, Producer = "Mancini", ProductName = "Tomates", Quantity = 13,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 7, Producer = "Mancini", ProductName = "Pommes", Quantity = 14,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 8, Producer = "Favre", ProductName = "Poires", Quantity = 5,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 8, Producer = "Favre", ProductName = "Pastèques", Quantity = 5,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 8, Producer = "Favre", ProductName = "Haricots", Quantity = 5,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 8, Producer = "Favre", ProductName = "Courges", Quantity = 17,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 8, Producer = "Favre", ProductName = "Tomates", Quantity = 9,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 9, Producer = "Bovay", ProductName = "Pommes", Quantity = 13,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 9, Producer = "Bovay", ProductName = "Poires", Quantity = 5,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 9, Producer = "Bovay", ProductName = "Pastèques", Quantity = 20,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 9, Producer = "Bovay", ProductName = "Melons", Quantity = 20,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 9, Producer = "Bovay", ProductName = "Noix", Quantity = 13,Unit = "sac", PricePerUnit = 5.50 },
    new Product { Location = 10, Producer = "Cherix", ProductName = "Raisin", Quantity = 8,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 10, Producer = "Cherix", ProductName = "Pruneaux", Quantity = 19,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 10, Producer = "Cherix", ProductName = "Myrtilles", Quantity = 9,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 10, Producer = "Cherix", ProductName = "Groseilles", Quantity = 10,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 10, Producer = "Cherix", ProductName = "Pêches", Quantity = 9,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 11, Producer = "Beaud", ProductName = "Haricots", Quantity = 19,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 11, Producer = "Beaud", ProductName = "Courges", Quantity = 16,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 11, Producer = "Beaud", ProductName = "Tomates", Quantity = 18,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 11, Producer = "Beaud", ProductName = "Pommes", Quantity = 8,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 11, Producer = "Beaud", ProductName = "Poires", Quantity = 13,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 12, Producer = "Corbaz", ProductName = "Pastèques", Quantity = 15,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 12, Producer = "Corbaz", ProductName = "Melons", Quantity = 12,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 12, Producer = "Corbaz", ProductName = "Noix", Quantity = 11,Unit = "sac", PricePerUnit = 5.50 },
    new Product { Location = 12, Producer = "Corbaz", ProductName = "Raisin", Quantity = 16,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 12, Producer = "Corbaz", ProductName = "Pruneaux", Quantity = 20,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 13, Producer = "Amaudruz", ProductName = "Myrtilles", Quantity = 18,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 13, Producer = "Amaudruz", ProductName = "Groseilles", Quantity = 19,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 13, Producer = "Amaudruz", ProductName = "Pêches", Quantity = 12,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 13, Producer = "Amaudruz", ProductName = "Haricots", Quantity = 13,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 13, Producer = "Amaudruz", ProductName = "Courges", Quantity = 7,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 14, Producer = "Bühlmann", ProductName = "Tomates", Quantity = 12,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 14, Producer = "Bühlmann", ProductName = "Pommes", Quantity = 17,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 14, Producer = "Bühlmann", ProductName = "Poires", Quantity = 7,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 14, Producer = "Bühlmann", ProductName = "Pastèques", Quantity = 11,Unit = "pièce", PricePerUnit = 5.50 },
    new Product { Location = 14, Producer = "Bühlmann", ProductName = "Melons", Quantity = 7,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 15, Producer = "Crizzi", ProductName = "Noix", Quantity = 10,Unit = "sac", PricePerUnit = 5.50 },
    new Product { Location = 15, Producer = "Crizzi", ProductName = "Raisin", Quantity = 17,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 15, Producer = "Crizzi", ProductName = "Pruneaux", Quantity = 18,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 15, Producer = "Crizzi", ProductName = "Myrtilles", Quantity = 12,Unit = "kg", PricePerUnit = 5.50 },
    new Product { Location = 15, Producer = "Crizzi", ProductName = "Groseilles", Quantity = 12,Unit = "kg", PricePerUnit = 5.50 }
            };

    // Fonction d’affinité fournie
    static int Affinity(string name, string product)
    {
        return name.GroupBy(letter => letter)
            .Union(product.GroupBy(letter => letter))
            .Sum(group => group.Count());
    }

    static void Main()
    {
        if (!products.Any())
        {
            Console.WriteLine("Aucun produit fourni. Veuillez remplir la liste 'products'.");
            return;
        }

        // Normalisation légère
        var items = products.Select(p => new
        {
            p.Location,
            Producer = (p.Producer ?? "").Trim(),
            ProductName = (p.ProductName ?? "").Trim(),
            p.Quantity,
            p.Unit,
            p.PricePerUnit
        }).ToList();

        // 0) Quantité totale de groseilles
        int totalGroseilles = items.Aggregate(
            0,
            (acc, p) => acc + (string.Equals(p.ProductName, "Groseilles", StringComparison.OrdinalIgnoreCase) ? p.Quantity : 0)
        );

        // 1) CA total par marchand
        var revenueByMerchant = items
            .GroupBy(p => p.Producer)
            .Select(g => new
            {
                Producer = g.Key,
                Revenue = g.Aggregate(0.0, (acc, p) => acc + (p.Quantity * p.PricePerUnit))
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        // 2) Min / Max / Moyenne des CA — robustes même si aucun marchand
        double? revenueMin = revenueByMerchant.Any() ? revenueByMerchant.Min(x => x.Revenue) : (double?)null;
        double? revenueMax = revenueByMerchant.Any() ? revenueByMerchant.Max(x => x.Revenue) : (double?)null;
        double revenueAvg = revenueByMerchant.Any() ? revenueByMerchant.Average(x => x.Revenue) : 0.0;

        var lowRevenue = (revenueMin is null) ? null : revenueByMerchant.First(r => r.Revenue == revenueMin.Value);
        var topRevenue = (revenueMax is null) ? null : revenueByMerchant.First(r => r.Revenue == revenueMax.Value);

        // 3) Marchand avec le plus de noix
        var walnutsLeader = items
            .Where(p => string.Equals(p.ProductName, "Noix", StringComparison.OrdinalIgnoreCase))
            .GroupBy(p => p.Producer)
            .Select(g => new
            {
                Producer = g.Key,
                TotalWalnuts = g.Aggregate(0, (acc, p) => acc + p.Quantity)
            })
            .OrderByDescending(x => x.TotalWalnuts)
            .FirstOrDefault();

        // 4) Affinité totale par marchand
        var affinityByMerchant = items
            .GroupBy(p => p.Producer)
            .Select(g => new
            {
                Producer = g.Key,
                AffinityScore = g.Aggregate(0, (acc, p) =>
                    acc + Affinity(g.Key.ToLowerInvariant(), p.ProductName.ToLowerInvariant()))
            })
            .OrderByDescending(x => x.AffinityScore)
            .ToList();

        var topAffinity = affinityByMerchant.FirstOrDefault();

        // --- Affichages ---
        Console.WriteLine("== RÉSUMÉ ==");
        Console.WriteLine($"0) Groseilles disponibles : {totalGroseilles}");

        Console.WriteLine("\n1) CA total par marchand :");
        if (revenueByMerchant.Any())
        {
            foreach (var r in revenueByMerchant)
                Console.WriteLine($"   - {r.Producer} : {r.Revenue:F2}");
        }
        else
        {
            Console.WriteLine("   (aucun marchand)");
        }

        Console.WriteLine("\n2) Statistiques des CA :");
        if (revenueByMerchant.Any())
        {
            Console.WriteLine($"   Min : {revenueMin!.Value:F2} (chez {lowRevenue!.Producer})");
            Console.WriteLine($"   Max : {revenueMax!.Value:F2} (chez {topRevenue!.Producer})");
            Console.WriteLine($"   Moyenne : {revenueAvg:F2}");
        }
        else
        {
            Console.WriteLine("   (indisponibles — aucun marchand)");
        }

        Console.WriteLine("\n3) Plus de noix :");
        if (walnutsLeader is null)
            Console.WriteLine("   (aucun produit 'Noix' trouvé)");
        else
            Console.WriteLine($"   {walnutsLeader.Producer} ({walnutsLeader.TotalWalnuts})");

        Console.WriteLine("\n4) Affinité maximale :");
        if (topAffinity is null)
            Console.WriteLine("   (aucune donnée)");
        else
            Console.WriteLine($"   {topAffinity.Producer} (score {topAffinity.AffinityScore})");
    }
}
