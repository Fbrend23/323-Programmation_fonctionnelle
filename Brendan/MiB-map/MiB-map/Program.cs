using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace mib
{
    internal class Program
    {
        static void Main()
        {
            // === 1) Vos données existantes ==========================
            // Remplacez cette ligne par votre vraie liste `products` existante.
            var products = GetSampleProducts(); // <-- si vous avez déjà 'products', supprimez cet appel

            // === 2) TRANSFORMATION – version de référence (classe typée) ===
            var rowsTyped = products.Select(p =>
            {
                var anon = AnonymizeStrong(p.Producer);
                var category = Categorize(p.Quantity);
                var adjustedUnit = AdjustedUnitPrice(p.PricePerUnit, category);
                var caAdjusted = p.Quantity * adjustedUnit;
                var premium = caAdjusted > 100 ? "Premium" : "Standard";

                return new DashboardRow
                {
                    producer = anon,
                    productName = p.ProductName,
                    quantity = p.Quantity,
                    unit = p.Unit,
                    baseUnitPrice = p.PricePerUnit,
                    adjustedUnitPrice = adjustedUnit,
                    stockCategory = category,
                    ca = Math.Round(caAdjusted, 2),
                    profitability = premium
                };
            }).ToList();

            // === 3) EXPORT JSON =====================================
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(rowsTyped, jsonOptions);
            File.WriteAllText("dashboard.json", json, Encoding.UTF8);
            Console.WriteLine($"✅ Export JSON -> {Path.GetFullPath("dashboard.json")}");

            // Affichage console (quelques lignes)
            foreach (var r in rowsTyped.Take(5))
                Console.WriteLine($"{r.producer} | {r.productName} | {r.quantity} {r.unit} | base:{r.baseUnitPrice:F2} | adj:{r.adjustedUnitPrice:F2} | {r.stockCategory} | CA:{r.ca:F2} | {r.profitability}");

            // === 4) MESURES DE PERFORMANCES =========================
            // Pré-parations pour approches alternatives
            // A) Select simple (tout inline)
            Action approachA_SelectSimple = () =>
            {
                var _ = products.Select(p =>
                {
                    var cat = Categorize(p.Quantity);
                    var adj = AdjustedUnitPrice(p.PricePerUnit, cat);
                    var ca = p.Quantity * adj;
                    return ca;
                }).ToList();
            };

            // B) Select + méthodes externes (déjà utilisé ci-dessus)
            Action approachB_Methods = () =>
            {
                var _ = products.Select(p =>
                {
                    var cat = Categorize(p.Quantity);
                    var adj = AdjustedUnitPrice(p.PricePerUnit, cat);
                    var ca = p.Quantity * adj;
                    var prem = ca > 100 ? "Premium" : "Standard";
                    return new DashboardRow
                    {
                        producer = AnonymizeStrong(p.Producer),
                        productName = p.ProductName,
                        quantity = p.Quantity,
                        unit = p.Unit,
                        baseUnitPrice = p.PricePerUnit,
                        adjustedUnitPrice = adj,
                        stockCategory = cat,
                        ca = ca,
                        profitability = prem
                    };
                }).ToList();
            };

            // C) Select + expressions lambda “compactes”
            Func<Product, string> fAnon = p => AnonymizeStrong(p.Producer);
            Func<int, string> fCat = q => Categorize(q);
            Func<double, string, double> fAdj = (ppu, cat) => AdjustedUnitPrice(ppu, cat);
            Action approachC_Lambdas = () =>
            {
                var _ = products.Select(p =>
                {
                    var cat = fCat(p.Quantity);
                    var adj = fAdj(p.PricePerUnit, cat);
                    var ca = p.Quantity * adj;
                    return new DashboardRow
                    {
                        producer = fAnon(p),
                        productName = p.ProductName,
                        quantity = p.Quantity,
                        unit = p.Unit,
                        baseUnitPrice = p.PricePerUnit,
                        adjustedUnitPrice = adj,
                        stockCategory = cat,
                        ca = ca,
                        profitability = ca > 100 ? "Premium" : "Standard"
                    };
                }).ToList();
            };

            // D) Select vers objets anonymes vs classes typées
            Action approachD_AnonymousObjects = () =>
            {
                var _ = products.Select(p =>
                {
                    var cat = Categorize(p.Quantity);
                    var adj = AdjustedUnitPrice(p.PricePerUnit, cat);
                    var ca = p.Quantity * adj;
                    var prem = ca > 100 ? "Premium" : "Standard";

                    return new
                    {
                        producer = AnonymizeStrong(p.Producer),
                        productName = p.ProductName,
                        quantity = p.Quantity,
                        unit = p.Unit,
                        baseUnitPrice = p.PricePerUnit,
                        adjustedUnitPrice = adj,
                        stockCategory = cat,
                        ca,
                        profitability = prem
                    };
                }).ToList();
            };

            // E) Select simple (liste matérielle) + sérialisation JSON
            Action approachE_Typed_Json = () =>
            {
                var list = products.Select(p =>
                {
                    var cat = Categorize(p.Quantity);
                    var adj = AdjustedUnitPrice(p.PricePerUnit, cat);
                    var ca = p.Quantity * adj;
                    var prem = ca > 100 ? "Premium" : "Standard";
                    return new DashboardRow
                    {
                        producer = AnonymizeStrong(p.Producer),
                        productName = p.ProductName,
                        quantity = p.Quantity,
                        unit = p.Unit,
                        baseUnitPrice = p.PricePerUnit,
                        adjustedUnitPrice = adj,
                        stockCategory = cat,
                        ca = ca,
                        profitability = prem
                    };
                }).ToList();

                var _ = JsonSerializer.Serialize(list);
            };

            Action approachF_Anon_Json = () =>
            {
                var list = products.Select(p =>
                {
                    var cat = Categorize(p.Quantity);
                    var adj = AdjustedUnitPrice(p.PricePerUnit, cat);
                    var ca = p.Quantity * adj;
                    var prem = ca > 100 ? "Premium" : "Standard";
                    return new
                    {
                        producer = AnonymizeStrong(p.Producer),
                        productName = p.ProductName,
                        quantity = p.Quantity,
                        unit = p.Unit,
                        baseUnitPrice = p.PricePerUnit,
                        adjustedUnitPrice = adj,
                        stockCategory = cat,
                        ca,
                        profitability = prem
                    };
                }).ToList();

                var _ = JsonSerializer.Serialize(list);
            };

            // Mesures
            var results = new List<PerfResult>
            {
                Measure("A - Select simple", approachA_SelectSimple, 2_000),
                Measure("B - Méthodes externes (typé)", approachB_Methods, 2_000),
                Measure("C - Lambdas (typé)", approachC_Lambdas, 2_000),
                Measure("D - Objets anonymes", approachD_AnonymousObjects, 2_000),
                Measure("E - Typé + JSON", approachE_Typed_Json, 1_000),
                Measure("F - Anonyme + JSON", approachF_Anon_Json, 1_000),
            };

            // Générer un rapport Markdown
            var md = new StringBuilder();
            md.AppendLine("# Benchmark Dashboard – Transformations & Export");
            md.AppendLine();
            md.AppendLine("| Approche | Itérations | Temps (ms) | Δ Mémoire (octets) |");
            md.AppendLine("|---|---:|---:|---:|");
            foreach (var r in results)
                md.AppendLine($"| {r.Name} | {r.Iterations} | {r.TimeMs} | {r.MemoryDelta} |");

            md.AppendLine();
            md.AppendLine("## Recommandations");
            md.AppendLine("- **Classes typées** (+ méthodes externes) offrent lisibilité et stabilité du contrat de données (sérialisation facile).");
            md.AppendLine("- **Objets anonymes** sont souvent *légèrement* plus rapides à matérialiser, mais moins pratiques à faire évoluer et documenter.");
            md.AppendLine("- **Lambdas vs méthodes** : performances proches ; privilégiez la **clarté**. Les méthodes nommées facilitent les tests.");
            md.AppendLine("- **JSON** : la sérialisation domine souvent le coût ; regroupez calculs + sérialisation pour limiter les allocations temporaires.");
            md.AppendLine("- En cas de **très grands volumes**, évitez les multiples passages sur les données et pré-allouez les listes si possible (capacity).");

            File.WriteAllText("performance.md", md.ToString(), Encoding.UTF8);
            Console.WriteLine($"✅ Rapport perf -> {Path.GetFullPath("performance.md")}");
        }

        // ===================== Aides métier =========================

        // Anonymisation renforcée : Premier caractère + (len-2) + dernier caractère
        // Ex: "Dumont" -> "D5t"; "Al" -> "A0l"; "A" -> "A0A"; "" -> ""
        static string AnonymizeStrong(string name)
        {
            if (string.IsNullOrEmpty(name)) return name ?? "";
            if (name.Length == 1) return $"{name[0]}0{name[0]}";
            int middleCount = Math.Max(0, name.Length - 2);
            return $"{name[0]}{middleCount}{name[^1]}";
        }

        // Catégorisation selon la quantité
        static string Categorize(int quantity)
        {
            if (quantity < 10) return "Stock faible";
            if (quantity <= 15) return "Stock normal";
            return "Stock élevé";
        }

        // Majoration prix unitaire selon catégorie
        static double AdjustedUnitPrice(double basePrice, string category)
        {
            return category switch
            {
                "Stock faible" => basePrice * 1.15,
                "Stock normal" => basePrice * 1.05,
                _ => basePrice, // Stock élevé
            };
        }

        // ===================== Mesures perf =========================

        static (long timeMs, long memoryBytes) MesurePerf(Action action, int iterations = 1000)
        {
            // Forcer GC avant mesure
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memoryBefore = GC.GetTotalMemory(false);
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
                action();

            sw.Stop();
            long memoryAfter = GC.GetTotalMemory(false);

            return (sw.ElapsedMilliseconds, memoryAfter - memoryBefore);
        }

        static PerfResult Measure(string name, Action action, int iterations)
        {
            var (t, m) = MesurePerf(action, iterations);
            return new PerfResult { Name = name, Iterations = iterations, TimeMs = t, MemoryDelta = m };
        }

        // ===================== Modèles ==============================

        internal class Product
        {
            public int Location { get; set; }
            public string Producer { get; set; } = "";
            public string ProductName { get; set; } = "";
            public int Quantity { get; set; }
            public string Unit { get; set; } = "";
            public double PricePerUnit { get; set; }
        }

        // Ligne exportable JSON
        internal class DashboardRow
        {
            public string producer { get; set; } = "";
            public string productName { get; set; } = "";
            public int quantity { get; set; }
            public string unit { get; set; } = "";
            public double baseUnitPrice { get; set; }
            public double adjustedUnitPrice { get; set; }
            public string stockCategory { get; set; } = "";
            public double ca { get; set; }
            public string profitability { get; set; } = "";
        }

        internal class PerfResult
        {
            public string Name { get; set; } = "";
            public int Iterations { get; set; }
            public long TimeMs { get; set; }
            public long MemoryDelta { get; set; }
        }

        // ====== Données d’exemple ======
        static List<Product> GetSampleProducts()
        {
            return new List<Product>
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
        }
    }
}
