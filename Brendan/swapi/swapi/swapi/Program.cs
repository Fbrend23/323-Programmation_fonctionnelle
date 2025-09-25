using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// =========================
// SWAPI Explorer (Console)
// .NET 6+
// =========================
//
// Points clés:
// - HttpClient global réutilisé (socket + perf)
// - Désérialisation System.Text.Json (natif .NET)
// - Caching agressif pour réduire les appels API
// - Extensions pour affichage rapide
// - Commentaires ciblés pour transmissibilité
//
// NB: SWAPI peut renvoyer "unknown", "n/a", valeurs non numériques -> on filtre
// =========================

#region Http + JSON infra

public static class Swapi
{
    private static readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://swapi.dev/api/"),
        Timeout = TimeSpan.FromSeconds(30),
        DefaultRequestHeaders =
        {
            Accept = { new MediaTypeWithQualityHeaderValue("application/json") },
            UserAgent = { new ProductInfoHeaderValue("SwapiExplorer", "1.0") }
        }
    };

    // Cache simple (clé=URL absolue) pour éviter des hits répétitifs
    private static readonly Dictionary<string, string> _jsonCache = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static async Task<T> GetAsync<T>(string relativeOrAbsoluteUrl)
    {
        string url = relativeOrAbsoluteUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? relativeOrAbsoluteUrl
            : new Uri(_http.BaseAddress!, relativeOrAbsoluteUrl).ToString();

        if (_jsonCache.TryGetValue(url, out var cached))
            return JsonSerializer.Deserialize<T>(cached, _jsonOptions)!;

        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        _jsonCache[url] = json;
        return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
    }

    // Pagination générique (people, planets, starships, films…)
    public static async IAsyncEnumerable<TItem> GetAllAsync<TItem>(string resourcePath)
    {
        string? next = resourcePath;
        while (!string.IsNullOrEmpty(next))
        {
            var page = await GetAsync<PagedResult<TItem>>(next);
            foreach (var item in page.Results)
                yield return item;
            next = page.Next;
        }
    }

    // Petits helpers conviviaux
    public static Task<FilmIndex> FilmsAsync() => GetAsync<FilmIndex>("films");
    public static IAsyncEnumerable<Person> PeopleAllAsync() => GetAllAsync<Person>("people");
    public static IAsyncEnumerable<Planet> PlanetsAllAsync() => GetAllAsync<Planet>("planets");
    public static IAsyncEnumerable<Starship> StarshipsAllAsync() => GetAllAsync<Starship>("starships");
}

#endregion

#region Models (alignés sur SWAPI)

public sealed class PagedResult<T>
{
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("next")] public string? Next { get; set; }
    [JsonPropertyName("previous")] public string? Previous { get; set; }
    [JsonPropertyName("results")] public List<T> Results { get; set; } = new();
}

public sealed class FilmIndex
{
    public int Count { get; set; }
    public List<Film> Results { get; set; } = new();
}

public sealed class Film
{
    public string Title { get; set; } = "";
    public string Opening_Crawl { get; set; } = "";
    public int Episode_Id { get; set; }
    public string Director { get; set; } = "";
    public string Producer { get; set; } = "";
    public string Release_Date { get; set; } = "";
    public string Url { get; set; } = "";
    public List<string> Characters { get; set; } = new();
    public List<string> Planets { get; set; } = new();
    public List<string> Starships { get; set; } = new();

    public override string ToString() => $"{Title} (Ep. {Episode_Id})";
}

public sealed class Person
{
    public string Name { get; set; } = "";
    public List<string> Films { get; set; } = new();
    public List<string> Starships { get; set; } = new();
    public override string ToString() => Name;
}

public sealed class Planet
{
    public string Name { get; set; } = "";
    public string Population { get; set; } = "unknown"; // string dans SWAPI
    public override string ToString() => $"{Name} (pop: {Population})";
}

public sealed class Starship
{
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Cost_In_Credits { get; set; } = "unknown";
    public string Length { get; set; } = "unknown";
    public string Max_Atmosphering_Speed { get; set; } = "n/a";
    public string Hyperdrive_Rating { get; set; } = "unknown";
    public List<string> Films { get; set; } = new();
    public List<string> Pilots { get; set; } = new();

    public override string ToString() => $"{Name} | cost={Cost_In_Credits} | speed={Max_Atmosphering_Speed} | hyper={Hyperdrive_Rating}";
}

#endregion

#region Extensions utilitaires

public static class EnumerableExtensions
{
    /// <summary>
    /// Affichage rapide d’une séquence (une ligne par élément).
    /// Repose sur ToString() des éléments.
    /// </summary>
    public static void Write<T>(this IEnumerable<T> sequence, string? title = null)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine(title);
            Console.WriteLine(new string('-', title.Length));
        }
        foreach (var item in sequence)
            Console.WriteLine(item);
        Console.WriteLine();
    }
}

#endregion

#region Conversions robustes

public static class SafeParse
{
    public static bool TryParseLong(string? s, out long value)
    {
        // SWAPI peut renvoyer "unknown", "n/a", "1,600", "36.8"
        if (string.IsNullOrWhiteSpace(s)) { value = 0; return false; }
        s = s.ToLowerInvariant().Trim();
        if (s == "unknown" || s == "n/a") { value = 0; return false; }

        // retirer virgules et espaces
        var normalized = s.Replace(",", "").Trim();
        return long.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDouble(string? s, out double value)
    {
        if (string.IsNullOrWhiteSpace(s)) { value = 0; return false; }
        s = s.ToLowerInvariant().Trim();
        if (s == "unknown" || s == "n/a") { value = 0; return false; }

        var normalized = s.Replace(",", "").Trim();
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}

#endregion

#region Calculs métier (réponses Planète 1)

public static class Questions
{
    /// Q1: Film au titre le plus long
    public static Film FilmAuTitreLePlusLong(IEnumerable<Film> films)
        => films.OrderByDescending(f => f.Title.Length).First();

    /// Q2: Personnage présent dans le plus de films
    public static Person PersonnageLePlusPresent(IEnumerable<Person> persons)
        => persons.OrderByDescending(p => p.Films.Count).First();

    /// Q3: Planète la plus peuplée (ignore "unknown")
    public static Planet PlaneteLaPlusPeuplee(IEnumerable<Planet> planets)
        => planets
            .Select(p => (planet: p, pop: SafeParse.TryParseLong(p.Population, out var n) ? n : -1))
            .Where(t => t.pop >= 0)
            .OrderByDescending(t => t.pop)
            .First().planet;

    /// Q4: Nombre de X-Wing achetables en vendant un Star Destroyer
    public static (long count, long starDestroyerCost, long xwingCost) CombienDeXWing(IEnumerable<Starship> ships)
    {
        var sd = ships.FirstOrDefault(s => s.Name.Equals("Star Destroyer", StringComparison.OrdinalIgnoreCase));
        var xw = ships.FirstOrDefault(s => s.Name.Equals("X-wing", StringComparison.OrdinalIgnoreCase));

        if (sd is null || xw is null) return (0, 0, 0);

        var okSd = SafeParse.TryParseLong(sd.Cost_In_Credits, out var sdCost);
        var okXw = SafeParse.TryParseLong(xw.Cost_In_Credits, out var xwCost);
        if (!okSd || !okXw || xwCost == 0) return (0, sdCost, xwCost);

        return (sdCost / xwCost, sdCost, xwCost);
    }

    /// Q5: Obi-Wan peut-il piloter le Millennium Falcon ?
    /// -> on regarde si "Obi-Wan Kenobi" apparaît dans la liste des "pilots" du Falcon
    public static bool ObiWanPeutPiloterMillenniumFalcon(IEnumerable<Person> persons, IEnumerable<Starship> ships)
    {
        var falcon = ships.FirstOrDefault(s => s.Name.Equals("Millennium Falcon", StringComparison.OrdinalIgnoreCase));
        if (falcon is null) return false;

        // Dans SWAPI, Starship.Pilots = URLs de people
        var obi = persons.FirstOrDefault(p => p.Name.Equals("Obi-Wan Kenobi", StringComparison.OrdinalIgnoreCase));
        if (obi is null) return false;

        // On compare via URLs (pilots contiennent des URLs /people/{id}/)
        // Solution simple: pour chaque URL de pilote du Falcon, on charge la personne et check son nom
        // (mais on peut également comparer par "contains" sur l’ID d’Obi-Wan si on le résout)
        foreach (var pilotUrl in falcon.Pilots)
        {
            var pilot = Swapi.GetAsync<Person>(pilotUrl).GetAwaiter().GetResult();
            if (pilot.Name.Equals("Obi-Wan Kenobi", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false; // En pratique, la réponse attendue est "non".
    }

    /// Q6: Vaisseau le plus rapide en "vitesse lumière"
    /// Hypothèse: "vmax" = max_atmosphering_speed * (1 / hyperdrive_rating)
    /// (car hyperdrive_rating plus petit => vaisseau plus rapide).
    public static Starship VaisseauPlusRapideLumiere(IEnumerable<Starship> ships)
    {
        return ships
            .Select(s =>
            {
                var hasSpeed = SafeParse.TryParseDouble(s.Max_Atmosphering_Speed, out var atmos);
                var hasHyper = SafeParse.TryParseDouble(s.Hyperdrive_Rating, out var hyper);
                double score = (hasSpeed && hasHyper && hyper > 0) ? atmos * (1.0 / hyper) : double.MinValue;
                return (ship: s, score);
            })
            .OrderByDescending(t => t.score)
            .First().ship;
    }

    /// Q7: Nombre de vaisseaux plus rapides que la moyenne de la vitesse atmosphérique
    public static (int count, double average) PlusRapidesQueMoyenneAtmos(IEnumerable<Starship> ships)
    {
        var speeds = ships
            .Select(s => SafeParse.TryParseDouble(s.Max_Atmosphering_Speed, out var v) ? v : double.NaN)
            .Where(v => !double.IsNaN(v))
            .ToList();

        if (speeds.Count == 0) return (0, 0);

        var avg = speeds.Average();

        var count = ships.Count(s =>
            SafeParse.TryParseDouble(s.Max_Atmosphering_Speed, out var v) && v > avg);

        return (count, avg);
    }

    /// Q8: Budget total de la flotte en CHF (1 crédit = 0.778 CHF)
    public static (decimal totalCredits, decimal totalChf) BudgetTotalChf(IEnumerable<Starship> ships)
    {
        const decimal rate = 0.778m;
        var totalCredits = ships
            .Select(s => SafeParse.TryParseLong(s.Cost_In_Credits, out var c) ? (decimal)c : 0m)
            .Sum();

        return (totalCredits, totalCredits * rate);
    }

    /// Q9: Générer CSV vaisseaux.txt (nom;prix;longueur;films;planetes_survolees)
    /// - Films: noms en minuscules séparés par des tirets
    /// - Planètes survolées: union des planètes des films où le vaisseau apparaît
    public static async Task GenereCsvVaisseauxAsync(IEnumerable<Starship> ships, string outputPath)
    {
        // Pré-chargement index film URL -> titre, et film URL -> planètes
        var filmIndex = await Swapi.FilmsAsync();
        var filmByUrl = filmIndex.Results.ToDictionary(
            f => f.Url.Trim(),
            f => f,
            StringComparer.OrdinalIgnoreCase);

        // Alternative robuste : reconstruire le mapping en résolvant chaque URL de starship.Films
        async Task<Film> ResolveFilmAsync(string url) => await Swapi.GetAsync<Film>(url);

        var sb = new StringBuilder();
        sb.AppendLine("nom;prix;longueur;films;planetes_survolees");

        foreach (var s in ships)
        {
            var price = SafeParse.TryParseLong(s.Cost_In_Credits, out var p) ? p.ToString(CultureInfo.InvariantCulture) : "unknown";
            var length = s.Length?.Replace(",", "", StringComparison.OrdinalIgnoreCase) ?? "unknown";

            // Noms de films (minuscules, tirets)
            var filmTitles = new List<string>();
            var planetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var filmUrl in s.Films)
            {
                var film = await ResolveFilmAsync(filmUrl);
                filmTitles.Add(film.Title.ToLowerInvariant().Replace(' ', '-'));

                // Ajouter les planètes de ce film
                foreach (var planetUrl in film.Planets)
                {
                    var planet = await Swapi.GetAsync<Planet>(planetUrl);
                    planetNames.Add(planet.Name.ToLowerInvariant().Replace(' ', '-'));
                }
            }

            var filmsJoined = string.Join('-', filmTitles);
            var planetsJoined = string.Join('-', planetNames);
            sb.AppendLine($"{s.Name};{price};{length};{filmsJoined};{planetsJoined}");
        }

        await File.WriteAllTextAsync(outputPath, sb.ToString(), Encoding.UTF8);
    }
}

#endregion

#region Planète 2 (Levenshtein + affiche film)

public static class Fuzzy
{
    // Implémentation simple de Levenshtein (O(n*m)), suffisante ici.
    // Pour la perf maximale, utiliser Quickenshtein (nuget).
    public static int Levenshtein(string a, string b)
    {
        a = a ?? "";
        b = b ?? "";
        var n = a.Length;
        var m = b.Length;
        var d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }
        return d[n, m];
    }

    public static async Task<(Film film, int distance)> TrouveFilmProcheAsync(string saisie, int seuil = 8)
    {
        var films = (await Swapi.FilmsAsync()).Results;
        var best = films
            .Select(f => (film: f, dist: Levenshtein(saisie.ToLowerInvariant(), f.Title.ToLowerInvariant())))
            .OrderBy(t => t.dist)
            .First();

        if (best.dist <= seuil) return best;
        throw new InvalidOperationException($"Aucun titre suffisamment proche (distance={best.dist} > seuil={seuil}).");
    }

    public static string AfficheFilm(Film film)
    {
        // On pourrait enrichir (durée non fournie par SWAPI; ici on affiche la date)
        var synopsis = film.Opening_Crawl.Replace("\r", " ").Replace("\n", " ").Trim();
        return
$@"Titre     : {film.Title}
Synopsis  : {synopsis}
Sortie    : {film.Release_Date}
Acteurs   : {film.Characters.Count} (voir SWAPI pour la liste détaillée)";
    }
}

#endregion

#region Planètes 3 & 4 (HTML et ouverture navigateur)

public static class Output
{
    public static async Task EcrireHtmlEtOuvrirAsync(string htmlPath, string htmlContent)
    {
        await File.WriteAllTextAsync(htmlPath, htmlContent, Encoding.UTF8);
        OuvrirDansNavigateur(htmlPath);
    }

    public static void OuvrirDansNavigateur(string pathOrUrl)
    {
        // Sur Windows, Process.Start avec UseShellExecute = true suffit (ouvre assoc par défaut)
        Process.Start(new ProcessStartInfo
        {
            FileName = pathOrUrl,
            UseShellExecute = true
        });
    }
}

#endregion

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("SWAPI Explorer — Hyperespace engagé.\n");

        // Chargements parallèles (perf)
        var filmsTask = Swapi.FilmsAsync();
        var peopleTask = Swapi.PeopleAllAsync().ToListAsync();
        var planetsTask = Swapi.PlanetsAllAsync().ToListAsync();
        var starshipsTask = Swapi.StarshipsAllAsync().ToListAsync();

        var films = (await filmsTask).Results;
        var persons = await peopleTask;
        var planets = await planetsTask;
        var starships = await starshipsTask;

        // ======= Réponses Planète 1 =======
        // Q1
        var filmTitrePlusLong = Questions.FilmAuTitreLePlusLong(films);
        Console.WriteLine($"Q1 — Titre le plus long : {filmTitrePlusLong.Title}");

        // Q2
        var persoLePlusPresent = Questions.PersonnageLePlusPresent(persons);
        Console.WriteLine($"Q2 — Personnage présent dans le plus de films : {persoLePlusPresent.Name} ({persoLePlusPresent.Films.Count} films)");

        // Q3
        var plusPeuplee = Questions.PlaneteLaPlusPeuplee(planets);
        Console.WriteLine($"Q3 — Planète la plus peuplée : {plusPeuplee.Name} (population {plusPeuplee.Population})");

        // Q4
        var (nbXWing, sdCost, xwCost) = Questions.CombienDeXWing(starships);
        Console.WriteLine($"Q4 — X-Wing achetables avec 1 Star Destroyer : {nbXWing} (SD={sdCost} cr, X-Wing={xwCost} cr)");

        // Q5
        var obiPeut = Questions.ObiWanPeutPiloterMillenniumFalcon(persons, starships);
        Console.WriteLine($"Q5 — Obi-Wan peut-il piloter le Millennium Falcon ? {(obiPeut ? "Oui" : "Non")}");

        // Q6
        var fastest = Questions.VaisseauPlusRapideLumiere(starships);
        Console.WriteLine($"Q6 — Vaisseau le plus rapide (v_lumière) : {fastest.Name}");

        // Q7
        var (countFaster, avgAtmos) = Questions.PlusRapidesQueMoyenneAtmos(starships);
        Console.WriteLine($"Q7 — Vaisseaux plus rapides que la vitesse atmosphérique moyenne ({avgAtmos:F1}) : {countFaster}");

        // Q8
        var (totalCredits, totalChf) = Questions.BudgetTotalChf(starships);
        Console.WriteLine($"Q8 — Budget flotte totale : {totalCredits:N0} crédits ≈ {totalChf:N0} CHF");

        // Q9 (CSV)
        var csvPath = Path.Combine(Environment.CurrentDirectory, "vaisseaux.txt");
        await Questions.GenereCsvVaisseauxAsync(starships, csvPath);
        Console.WriteLine($"Q9 — CSV généré : {csvPath}");

        // ======= Planète 2 (démo courte) =======
        Console.WriteLine("\nPlanète 2 — Saisis un titre approximatif (ou Enter pour ignorer):");
        var saisie = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(saisie))
        {
            try
            {
                var (film, dist) = await Fuzzy.TrouveFilmProcheAsync(saisie, seuil: 8);
                Console.WriteLine($"\nMeilleure correspondance (distance={dist}) :");
                Console.WriteLine(Fuzzy.AfficheFilm(film));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Aucun film reconnu: {ex.Message}");
            }
        }

        // ======= Planètes 3 & 4 (hooks) =======
        // Exemple très simple: génère une page HTML avec le meilleur film (titre + crawl)
        var bestFilm = filmTitrePlusLong; // on réutilise Q1
        var html = $@"<!DOCTYPE html>
<html lang=""fr"">
<head>
<meta charset=""utf-8"" />
<title>Billboard Star Wars</title>
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
<style>
body {{ font-family: system-ui, sans-serif; padding: 2rem; background: #0b0e1a; color: #e6e9f0; }}
h1 {{ margin: 0 0 1rem; }}
pre {{ white-space: pre-wrap; line-height: 1.6; background:#0f1326; padding:1rem; border-radius:12px; }}
.small {{ opacity:.8; font-size:.9rem }}
</style>
</head>
<body>
  <h1>{bestFilm.Title}</h1>
  <div class=""small"">Sortie: {bestFilm.Release_Date}</div>
  <h2>Opening crawl</h2>
  <pre>{bestFilm.Opening_Crawl}</pre>
</body>
</html>";
        var htmlPath = Path.Combine(Environment.CurrentDirectory, "billboard.html");
        await Output.EcrireHtmlEtOuvrirAsync(htmlPath, html);

        Console.WriteLine("\nHyperespace accompli. Que la Force soit avec toi.");
    }
}

// Petit helper LINQ async
public static class AsyncLinq
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source) list.Add(item);
        return list;
    }
}
