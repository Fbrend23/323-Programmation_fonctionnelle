string[] words = { "bonjour", "hello", "monde", "vert", "roxuge", "bleu", "jaune" };
Func<string, bool> hasNoX = word => !word.Contains("x");
Func<string, bool> lengthOfFour = word => word.Length >= 4;
var avg = (int)words.Average(s => s.Length);             
Func<string, bool> avgLength = word => word.Length == avg;

//Filtres
var filters = new List<Func<string, bool>>();
filters.Add(hasNoX);
filters.Add(lengthOfFour);
filters.Add(avgLength);

//Choix
Console.WriteLine($"Liste de mots : {String.Join(',', words)}");
Console.WriteLine("1. Pas de x");
Console.WriteLine("2. >= 4");
Console.WriteLine("3. = moyenne de longueur dans la liste");
Console.Write("\nChoix: ");

int choice = Convert.ToInt32(Console.ReadLine()) - 1;
var filtered = words.Where(filters[choice]);

Console.WriteLine($"Résultat: {String.Join(',', filtered)}");

choice = Convert.ToInt32(Console.ReadLine()) - 1;


