using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// =======================================
// SWAPI Explorer — Planète 1 (Q1 → Q9)
// .NET 6+ — Console
// =======================================
// Points clés
// - HttpClient global (perf + sockets réutilisés)
// - System.Text.Json natif
// - Cache JSON simple pour éviter les appels répétés
// - Parsing "robuste" des nombres (unknown, n/a, virgules…)
// - Commentaires ciblés et variables explicites
//
// Remarques sur les données SWAPI :
// - Beaucoup de champs numériques sont des strings ("unknown", "n/a")
// - max_atmosphering_speed peut contenir du texte ; hyperdrive_rating est un nombre, plus petit = plus rapide (canon)
// - L’énoncé Q6 demande vmax = vitesse atmos * ratio hyperespace (-> multiplication, on respecte l’énoncé)
// =======================================

#region HTTP + Infra JSON

public static class Swapi
{
    private static readonly HttpClient _http = new(
        new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        })
    {
        BaseAddress = new Uri("https://swapi.dev/api/"),
        Timeout = TimeSpan.FromSeconds(30)
    };

    static Swapi()
    {
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SwapiExplorer", "1.0"));
    }

    // Cache JSON (clé = URL absolue normalisée)
    private static readonly Dictionary<string, string> _jsonCache = new(StringComparer.OrdinalIgnoreCase);

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

        url = url.Trim();

        if (_jsonCache.TryGetValue(url, out var cached))
            return JsonSerializer.Deserialize<T>(cached, _jsonOptions)!;

        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        _jsonCache[url] = json;
        return JsonSerializer.Deserialize<T>(json, _jsonOptions)!;
    }

    // Pagination générique (people, planets, starships…)
    public static async IAsyncEnumerable<TItem> GetAllAsync<TItem>(string resourcePath)
    {
        string? next = resourcePath;
        while (!string.IsNullOrWhiteSpace(next))
        {
            var page = await GetAsync<PagedResult<TItem>>(next);
            foreach (var item in page.Results)
                yield return item;
            next = page.Next;
        }
    }

    // Raccourcis conviviaux
    public static Task<FilmIndex> FilmsAsync() => GetAsync<FilmIndex>("films");
    public static IAsyncEnumerable<Person> PeopleAllAsync() => GetAllAsync<Person>("people");
    public static IAsyncEnumerable<Planet> PlanetsAllAsync() => GetAllAsync<Planet>("planets");
    public static IAsyncEnumerable<Starship> StarshipsAllAsync() => GetAllAsync<Starship>("starships");
}

#endregion

#region Modèles (SWAPI)

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
    [JsonPropertyName("opening_crawl")] public string Opening_Crawl { get; set; } = "";
    [JsonPropertyName("episode_id")] public int Episode_Id { get; set; }
    public string Director { get; set; } = "";
    public string Producer { get; set; } = "";
    [JsonPropertyName("release_date")] public string Release_Date { get; set; } = "";
    public List<string> Characters { get; set; } = new();
    public List<string> Planets { get; set; } = new();
    public List<string> Starships { get; set; } = new();

    // URL officielle (utile pour les correspondances stables)
    public string Url { get; set; } = "";

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
    public string Population { get; set; } = "unknown"; // string côté SWAPI
    public override string ToString() => $"{Name} (pop: {Population})";
}

public sealed class Starship
{
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    [JsonPropertyName("cost_in_credits")] public string Cost_In_Credits { get; set; } = "unknown";
    public string Length { get; set; } = "unknown";
    [JsonPropertyName("max_atmosphering_speed")] public string Max_Atmosphering_Speed { get; set; } = "n/a";
    [JsonPropertyName("hyperdrive_rating")] public string Hyperdrive_Rating { get; set; } = "unknown";
    public List<string> Films { get; set; } = new();
    public List<string> Pilots { get; set; } = new();

    public override string ToString() => $"{Name} | cost={Cost_In_Credits} | speed={Max_Atmosphering_Speed} | hyper={Hyperdrive_Rating}";
}

#endregion

#region Extensions d'affichage

public static class EnumerableExtensions
{
    /// <summary>Affichage rapide d’une séquence (une ligne par élément).</summary>
    public static void Write<T>(this IEnumerable<T> sequence, string? title = null)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine(title);
            Console.WriteLine(new string('-', title.Length));
        }
        foreach (var item in sequence) Console.WriteLine(item);
        Console.WriteLine();
    }
}

#endregion

#region Parsing robuste

public static class SafeParse
{
    // Conserve uniquement chiffres et point décimal ; ignore "unknown"/"n/a"
    private static string KeepNumeric(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        s = s.ToLowerInvariant().Trim();
        if (s is "unknown" or "n/a") return "";
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
            if ((ch >= '0' && ch <= '9') || ch == '.') sb.Append(ch);
        return sb.ToString();
    }

    public static bool TryParseLong(string? s, out long value)
    {
        var normalized = KeepNumeric(s);
        if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
        {
            value = (long)Math.Round(d);
            return true;
        }
        value = 0; return false;
    }

    public static bool TryParseDouble(string? s, out double value)
    {
        var normalized = KeepNumeric(s);
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}

#endregion

#region Calculs (Planète 1)

public static class Questions
{
    // Q1: Film au titre le plus long
    public static Film FilmAuTitreLePlusLong(IEnumerable<Film> films)
        => films.OrderByDescending(f => f.Title.Length).First();

    // Q2: Personnage présent dans le plus de films
    public static List<Person> PersonnagesLesPlusPresents(IEnumerable<Person> persons)
    {
        if (persons is null) return new List<Person>();

        // Nombre max de films (tolère Films null)
        var max = persons.Any() ? persons.Max(p => p.Films?.Count ?? 0) : 0;

        // Tous les personnages ayant ce max, ordre déterministe par nom
        return persons
            .Where(p => (p.Films?.Count ?? 0) == max)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToList();
    }


    // Q3: Planète la plus peuplée (ignore "unknown")
    public static Planet PlaneteLaPlusPeuplee(IEnumerable<Planet> planets)
        => planets
            .Select(p => (p, pop: SafeParse.TryParseLong(p.Population, out var n) ? n : -1))
            .Where(t => t.pop >= 0)
            .OrderByDescending(t => t.pop)
            .First().p;

    // Q4: Nombre de X-Wing achetables en vendant un Star Destroyer
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

    // Q5: Obi-Wan Kenobi peut-il piloter le Millennium Falcon ?
    // -> On teste si son nom apparaît parmi les pilotes du Falcon (via URLs /people/{id}/)
    public static bool ObiWanPeutPiloterMillenniumFalcon(IEnumerable<Person> persons, IEnumerable<Starship> ships)
    {
        var falcon = ships.FirstOrDefault(s => s.Name.Equals("Millennium Falcon", StringComparison.OrdinalIgnoreCase));
        if (falcon is null) return false;

        foreach (var pilotUrl in falcon.Pilots)
        {
            var pilot = Swapi.GetAsync<Person>(pilotUrl).GetAwaiter().GetResult();
            if (pilot.Name.Equals("Obi-Wan Kenobi", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false; // Réponse attendue : non
    }

    // Q6: Vaisseau le plus rapide en "vitesse lumière"
    // Énoncé: vmax = vitesse atmosphérique max * ratio hyperespace
    // (NB canon: hyperdrive_rating plus petit => + rapide, mais on suit l'énoncé)
    public static Starship VaisseauPlusRapideLumiere(IEnumerable<Starship> ships)
    {
        return ships
            .Select(s =>
            {
                var hasSpeed = SafeParse.TryParseDouble(s.Max_Atmosphering_Speed, out var atmos);
                var hasHyper = SafeParse.TryParseDouble(s.Hyperdrive_Rating, out var hyper);
                double score = (hasSpeed && hasHyper) ? atmos * hyper : double.NaN;
                return (s, score);
            })
            .OrderByDescending(t => double.IsNaN(t.score) ? double.MinValue : t.score)
            .First().s;
    }

    // Q7: Combien de vaisseaux > moyenne des vitesses atmosphériques
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

    // Q8: Budget total de la flotte (1 crédit = 0.778 CHF)
    public static (decimal totalCredits, decimal totalChf) BudgetTotalChf(IEnumerable<Starship> ships)
    {
        const decimal tauxChf = 0.778m;
        var totalCredits = ships
            .Select(s => SafeParse.TryParseLong(s.Cost_In_Credits, out var c) ? (decimal)c : 0m)
            .Sum();
        return (totalCredits, totalCredits * tauxChf);
    }

    // Q9: Génère vaisseaux.txt
    // Format: nom;prix;longueur;films;planetes_survolees
    // - films: noms en minuscules séparés par des tirets
    // - planètes: union des planètes des films où le vaisseau apparaît (minuscule, tirets)
    public static async Task GenereCsvVaisseauxAsync(IEnumerable<Starship> ships, string outputPath)
    {
        // Index initial Films par URL (plus fiable)
        var filmIndex = await Swapi.FilmsAsync();
        var filmByUrl = filmIndex.Results.ToDictionary(f => f.Url.Trim(), f => f, StringComparer.OrdinalIgnoreCase);

        async Task<Film> ResolveFilmAsync(string url)
        {
            url = url.Trim();
            if (!filmByUrl.TryGetValue(url, out var f))
            {
                f = await Swapi.GetAsync<Film>(url);
                filmByUrl[url] = f;
            }
            return f;
        }

        var sb = new StringBuilder();
        sb.AppendLine("nom;prix;longueur;films;planetes_survolees");

        foreach (var ship in ships)
        {
            var prix = SafeParse.TryParseLong(ship.Cost_In_Credits, out var p) ? p.ToString(CultureInfo.InvariantCulture) : "unknown";

            string longueur = ship.Length ?? "unknown";
            if (SafeParse.TryParseDouble(ship.Length, out var lenVal))
                longueur = lenVal.ToString("0.###", CultureInfo.InvariantCulture);

            var filmTitles = new List<string>();
            var planetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var filmUrl in ship.Films)
            {
                var film = await ResolveFilmAsync(filmUrl);
                filmTitles.Add(film.Title.ToLowerInvariant().Replace(' ', '-'));

                foreach (var planetUrl in film.Planets)
                {
                    var planet = await Swapi.GetAsync<Planet>(planetUrl);
                    planetNames.Add(planet.Name.ToLowerInvariant().Replace(' ', '-'));
                }
            }

            var filmsJoined = string.Join('-', filmTitles.OrderBy(x => x, StringComparer.Ordinal));
            var planetsJoined = string.Join('-', planetNames.OrderBy(x => x, StringComparer.Ordinal));

            sb.AppendLine($"{ship.Name};{prix};{longueur};{filmsJoined};{planetsJoined}");
        }

        await File.WriteAllTextAsync(outputPath, sb.ToString(), Encoding.UTF8);
    }
}

#endregion

#region Programme

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("SWAPI Explorer — Hyper-espace engagé.\n");

        // Chargements parallèles (perf)
        var filmsTask = Swapi.FilmsAsync();
        var peopleTask = Swapi.PeopleAllAsync().ToListAsync();
        var planetsTask = Swapi.PlanetsAllAsync().ToListAsync();
        var starshipsTask = Swapi.StarshipsAllAsync().ToListAsync();

        var films = (await filmsTask).Results;
        var persons = await peopleTask;
        var planets = await planetsTask;
        var starships = await starshipsTask;
        var topPersons = Questions.PersonnagesLesPlusPresents(persons);
        var topCount = topPersons.FirstOrDefault()?.Films.Count ?? 0;
        // Q1
        var filmTitrePlusLong = Questions.FilmAuTitreLePlusLong(films);
        Console.WriteLine($"Q1 — Titre le plus long : {filmTitrePlusLong.Title}");

        // Q2
        Console.WriteLine(
            $"Q2 — Personnage(s) présent(s) dans le plus de films ({topCount} films) : {string.Join(", ", topPersons.Select(p => p.Name))}"
        );

        // Q3
        var planetePlusPeuplee = Questions.PlaneteLaPlusPeuplee(planets);
        Console.WriteLine($"Q3 — Planète la plus peuplée : {planetePlusPeuplee.Name} (population {planetePlusPeuplee.Population})");

        // Q4
        var (nbXWing, sdCost, xwCost) = Questions.CombienDeXWing(starships);
        Console.WriteLine($"Q4 — X-Wing achetables avec 1 Star Destroyer : {nbXWing} (SD={sdCost} cr, X-Wing={xwCost} cr)");

        // Q5
        var obiPeutPiloter = Questions.ObiWanPeutPiloterMillenniumFalcon(persons, starships);
        Console.WriteLine($"Q5 — Obi-Wan peut-il piloter le Millennium Falcon ? {(obiPeutPiloter ? "Oui" : "Non")}");

        // Q6
        var plusRapideLumiere = Questions.VaisseauPlusRapideLumiere(starships);
        Console.WriteLine($"Q6 — Vaisseau le plus rapide (v_lumière = atmos*hyper): {plusRapideLumiere.Name}");

        // Q7
        var (nbPlusRapides, moyenneAtmos) = Questions.PlusRapidesQueMoyenneAtmos(starships);
        Console.WriteLine($"Q7 — Vaisseaux > moyenne atmos ({moyenneAtmos:F1}) : {nbPlusRapides}");

        // Q8
        var (totalCredits, totalChf) = Questions.BudgetTotalChf(starships);
        Console.WriteLine($"Q8 — Budget flotte totale : {totalCredits:N0} crédits ≈ {totalChf:N0} CHF");

        // Q9
        var csvPath = Path.Combine(Environment.CurrentDirectory, "vaisseaux.txt");
        await Questions.GenereCsvVaisseauxAsync(starships, csvPath);
        Console.WriteLine($"Q9 — CSV généré : {csvPath}");

        Console.WriteLine("\nHyper-espace accompli. Que la Force soit avec toi.");
    }
}

#endregion

#region Helpers

public static class AsyncLinq
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source) list.Add(item);
        return list;
    }
}

#endregion
