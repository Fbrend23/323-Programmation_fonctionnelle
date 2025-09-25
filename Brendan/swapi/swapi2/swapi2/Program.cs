using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// =============================================================
// PROGRAMME PRINCIPAL
// =============================================================
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        var swapiService = new SwapiService();

        Console.WriteLine("--- Planète 1: Requêtes sur l'API SWAPI ---");
        await SolvePlanet1(swapiService);

        Console.WriteLine("\n\n--- Planète 2: Recherche de film interactive ---");
        var selectedFilm = await SolvePlanet2(swapiService);

        if (selectedFilm != null)
        {
            Console.WriteLine("\n\n--- Planète 3: Génération de l'affiche web ---");
            await SolvePlanet3(selectedFilm);

            Console.WriteLine("\n\n--- Planète 4: Génération du texte d'introduction défilant ---");
            await SolvePlanet4(selectedFilm);
        }

        Console.WriteLine("\n\nMission terminée. Appuyez sur une touche pour quitter.");
        Console.ReadKey();
    }

    #region Solveurs de planètes

    public static async Task SolvePlanet1(SwapiService service)
    {
        Console.WriteLine("Récupération des données, cela peut prendre un moment...");
        var allFilms = await service.GetAllAsync<Film>("films/");
        var allPeople = await service.GetAllAsync<Person>("people/");
        var allPlanets = await service.GetAllAsync<Planet>("planets/");
        var allStarships = await service.GetAllAsync<Starship>("starships/");
        Console.WriteLine("Données récupérées !");

        // 1. Quel est le film Star Wars dont le titre est le plus long ?
        var longestTitleFilm = allFilms.OrderByDescending(f => f.Title.Length).FirstOrDefault();
        Console.WriteLine($"\n1. Film avec le titre le plus long : {longestTitleFilm?.Title}");

        // 2. Quel est le personnage qui est présent dans le plus de films ?
        var mostFeaturedCharacter = allPeople
            .OrderByDescending(p => p.Films.Count)
            .FirstOrDefault();
        Console.WriteLine($"2. Personnage le plus présent : {mostFeaturedCharacter?.Name} ({mostFeaturedCharacter?.Films.Count} films)");

        // 3. Quelle est la planète la plus peuplée ?
        var mostPopulatedPlanet = allPlanets
            .Where(p => long.TryParse(p.Population, out _))
            .OrderByDescending(p => long.Parse(p.Population))
            .FirstOrDefault();
        Console.WriteLine($"3. Planète la plus peuplée : {mostPopulatedPlanet?.Name} ({mostPopulatedPlanet?.Population} habitants)");

        // 4. Combien de starfighter X-Wing est-ce que je peux m'acheter si je vends un Star Destroyer ?
        var starDestroyer = allStarships.FirstOrDefault(s => s.Name == "Star Destroyer");
        var xWing = allStarships.FirstOrDefault(s => s.Name == "X-wing");
        if (starDestroyer != null && xWing != null &&
            decimal.TryParse(starDestroyer.CostInCredits, out var destroyerCost) &&
            decimal.TryParse(xWing.CostInCredits, out var xwingCost) && xwingCost > 0)
        {
            var ratio = Math.Floor(destroyerCost / xwingCost);
            Console.WriteLine($"4. Pour 1 Star Destroyer, vous pouvez acheter {ratio} X-Wings.");
        }
        else
        {
            Console.WriteLine("4. Impossible de calculer le ratio de prix entre Star Destroyer et X-Wing.");
        }

        // 5. Est-ce qu'Obi-wan Kenobi peut piloter un Millennium Falcon ?
        var obiWan = allPeople.FirstOrDefault(p => p.Name == "Obi-Wan Kenobi");
        var millenniumFalcon = allStarships.FirstOrDefault(s => s.Name == "Millennium Falcon");
        if (obiWan != null && millenniumFalcon != null)
        {
            bool canPilot = millenniumFalcon.Pilots.Contains(obiWan.Url);
            Console.WriteLine($"5. Obi-Wan Kenobi peut-il piloter le Millennium Falcon ? {(canPilot ? "Oui" : "Non")}");
        }
    }

    public static async Task<Film> SolvePlanet2(SwapiService service)
    {
        Console.Write("Veuillez entrer un titre de film Star Wars : ");
        string userInput = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userInput))
        {
            Console.WriteLine("Aucun titre entré. Annulation.");
            return null;
        }

        var allFilms = await service.GetAllAsync<Film>("films/");

        var filmWithDistance = allFilms
            .Select(film => new
            {
                Film = film,
                Distance = Levenshtein.GetDistance(userInput.ToLower(), film.Title.ToLower())
            })
            .OrderBy(x => x.Distance)
            .FirstOrDefault();

        const int LevenshteinThreshold = 5;
        if (filmWithDistance != null && filmWithDistance.Distance <= LevenshteinThreshold)
        {
            Console.WriteLine($"Film trouvé : '{filmWithDistance.Film.Title}' (Distance: {filmWithDistance.Distance})");
            // CORRECTION: On utilise Console.WriteLine directement sur l'objet unique.
            // Ceci appelle implicitement la méthode .ToString() que nous avons définie.
            Console.WriteLine(filmWithDistance.Film);
            return filmWithDistance.Film;
        }

        Console.WriteLine("Aucun film correspondant trouvé.");
        return null;
    }

    public static async Task SolvePlanet3(Film film)
    {
        try
        {
            string templatePath = "billboard.html";
            if (!File.Exists(templatePath))
            {
                Console.WriteLine($"Erreur: Le fichier modèle '{templatePath}' est introuvable.");
                return;
            }

            string template = await File.ReadAllTextAsync(templatePath);

            // Simplification : les acteurs ne sont pas directement dans l'objet Film.
            // Il faudrait faire des requêtes supplémentaires pour les récupérer.
            // Pour cet exercice, nous laissons la section vide.
            string htmlContent = template
                .Replace("{{TITLE}}", film.Title)
                .Replace("{{SYNOPSIS}}", film.OpeningCrawl.Replace("\r\n", "<br/>"))
                .Replace("{{DURATION}}", "N/A") // Info non disponible dans l'API
                .Replace("{{ACTORS}}", "<li>Données non récupérées via API</li>")
                .Replace("{{IMAGE_NAME}}", $"sw-ep{film.EpisodeId}.jpg");

            string outputPath = "affiche_film.html";
            await File.WriteAllTextAsync(outputPath, htmlContent);

            Console.WriteLine($"Fichier '{outputPath}' généré. Tentative d'ouverture dans le navigateur...");

            // Lancer le processus pour ouvrir le fichier
            var p = new Process
            {
                StartInfo = new ProcessStartInfo(outputPath)
                {
                    UseShellExecute = true
                }
            };
            p.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Une erreur est survenue lors de la génération de l'affiche : {ex.Message}");
        }
    }

    public static async Task SolvePlanet4(Film film)
    {
        try
        {
            string templatePath = Path.Combine("crawler", "index.html");
            if (!File.Exists(templatePath))
            {
                Console.WriteLine($"Erreur: Le fichier modèle '{templatePath}' est introuvable. Assurez-vous qu'il existe dans un sous-dossier 'crawler'.");
                return;
            }
            string template = await File.ReadAllTextAsync(templatePath);

            string htmlContent = template.Replace("{{OPENING_CRAWL}}", film.OpeningCrawl.Replace("\r\n", "\n"));

            string outputPath = Path.Combine("crawler", "crawler_output.html");
            await File.WriteAllTextAsync(outputPath, htmlContent);

            Console.WriteLine($"Fichier '{outputPath}' généré. Tentative d'ouverture...");

            var p = new Process
            {
                StartInfo = new ProcessStartInfo(outputPath)
                {
                    UseShellExecute = true
                }
            };
            p.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Une erreur est survenue lors de la génération du crawler : {ex.Message}");
        }
    }
    #endregion
}

// =============================================================
// EXTENSIONS
// =============================================================
public static class Extensions
{
    /// <summary>
    /// Affiche chaque élément d'une collection sur une nouvelle ligne,
    /// en utilisant la méthode ToString() de l'objet.
    /// </summary>
    public static void Write<T>(this IEnumerable<T> source)
    {
        if (source == null) return;
        foreach (var item in source)
        {
            Console.WriteLine(item);
        }
    }
}

// =============================================================
// SERVICE API
// =============================================================
public class SwapiService
{
    private static readonly HttpClient _client = new HttpClient();
    private const string BaseUrl = "https://swapi.dev/api/";
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Récupère toutes les pages d'un endpoint donné.
    /// </summary>
    public async Task<List<T>> GetAllAsync<T>(string endpoint)
    {
        var allItems = new List<T>();
        string nextUrl = BaseUrl + endpoint;

        while (!string.IsNullOrEmpty(nextUrl))
        {
            var response = await _client.GetAsync(nextUrl);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var pagedResult = JsonSerializer.Deserialize<PagedResult<T>>(json, _jsonOptions);

            if (pagedResult?.Results != null)
            {
                allItems.AddRange(pagedResult.Results);
            }
            nextUrl = pagedResult?.Next;
        }
        return allItems;
    }
}


// =============================================================
// MODÈLES (DTOs)
// =============================================================
public class PagedResult<T>
{
    public int Count { get; set; }
    public string Next { get; set; }
    public List<T> Results { get; set; }
}

public class Film
{
    public string Title { get; set; }
    [JsonPropertyName("episode_id")]
    public int EpisodeId { get; set; }
    [JsonPropertyName("opening_crawl")]
    public string OpeningCrawl { get; set; }
    public string Director { get; set; }
    public string Producer { get; set; }
    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; }
    public List<string> Characters { get; set; }
    public List<string> Planets { get; set; }
    public List<string> Starships { get; set; }
    public List<string> Vehicles { get; set; }
    public List<string> Species { get; set; }
    public string Url { get; set; }

    public override string ToString()
    {
        return $"Film: {Title} (Episode {EpisodeId}) - Réalisé par {Director}";
    }
}

public class Person
{
    public string Name { get; set; }
    public List<string> Films { get; set; }
    public string Url { get; set; }

    public override string ToString() => $"Personnage: {Name}";
}

public class Planet
{
    public string Name { get; set; }
    public string Population { get; set; }
    public override string ToString() => $"Planète: {Name}, Population: {Population}";
}

public class Starship
{
    public string Name { get; set; }
    [JsonPropertyName("cost_in_credits")]
    public string CostInCredits { get; set; }
    public string Length { get; set; }
    [JsonPropertyName("max_atmosphering_speed")]
    public string MaxAtmospheringSpeed { get; set; }
    [JsonPropertyName("hyperdrive_rating")]
    public string HyperdriveRating { get; set; }
    public List<string> Films { get; set; }
    public List<string> Pilots { get; set; }
    public string Url { get; set; }

    public override string ToString() => $"Vaisseau: {Name}, Coût: {CostInCredits} crédits";
}

// =============================================================
// AIDE - DISTANCE DE LEVENSHTEIN
// Code adapté de https://gist.github.com/Davidblkx/e12ab0bb2aff7fd8072632b396538560
// =============================================================
public static class Levenshtein
{
    public static int GetDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 0; j <= m; d[0, j] = j++) ;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
}

