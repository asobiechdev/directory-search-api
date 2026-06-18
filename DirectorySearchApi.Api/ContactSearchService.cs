using DirectorySearchApi.Api.Models;

namespace DirectorySearchApi.Api.Services;

public interface IContactSearchService
{
    List<Contact> Search(string query);
}

public class ContactSearchService : IContactSearchService
{
    private readonly List<Contact> _contacts;

    public ContactSearchService()
    {
        // Dane seedowe — fikcyjne kontakty
        _contacts = new List<Contact>
        {
            new() { Id = 1, FirstName = "Jan", LastName = "Kowalski", PhoneNumber = "123456789", Department = "IT", Email = "jan.kowalski@company.com" },
            new() { Id = 2, FirstName = "Maria", LastName = "Nowak", PhoneNumber = "987654321", Department = "HR", Email = "maria.nowak@company.com" },
            new() { Id = 3, FirstName = "Piotr", LastName = "Wójcik", PhoneNumber = "555666777", Department = "Finance", Email = "piotr.wojcik@company.com" },
            new() { Id = 4, FirstName = "Anna", LastName = "Lewandowska", PhoneNumber = "444555666", Department = "IT", Email = "anna.lewandowska@company.com" },
            new() { Id = 5, FirstName = "Krzysztof", LastName = "Żurawski", PhoneNumber = "111222333", Department = "Sales", Email = "krzysztof.zurawski@company.com" },
        };
    }

    public List<Contact> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _contacts;

        var q = query.ToLowerInvariant().Trim();

        // Szukaj w FirstName, LastName, PhoneNumber, Department
        var results = _contacts
            .Where(c => 
                MatchesQuery(c.FirstName, q) ||
                MatchesQuery(c.LastName, q) ||
                MatchesQuery(c.PhoneNumber, q) ||
                MatchesQuery(c.Department, q))
            .OrderByDescending(c => ScoreMatch(c, q))
            .ToList();

        return results;
    }

    // Fuzzy matching — tolerancja literówek
    private bool MatchesQuery(string field, string query)
    {
        if (string.IsNullOrEmpty(field))
            return false;

        var f = field.ToLowerInvariant();

        // Dokładne dopasowanie
        if (f.Contains(query))
            return true;

        // Levenshtein distance — tolerancja literówek
        return LevenshteinDistance(f, query) <= 2;
    }

    // Priorytetyzacja wyników — dokładne dopasowanie wyżej
    private int ScoreMatch(Contact contact, string query)
    {
        int score = 0;

        if (contact.FirstName.ToLowerInvariant().Contains(query)) score += 100;
        if (contact.FirstName.ToLowerInvariant().StartsWith(query)) score += 50;

        if (contact.LastName.ToLowerInvariant().Contains(query)) score += 80;
        if (contact.LastName.ToLowerInvariant().StartsWith(query)) score += 40;

        if (contact.PhoneNumber.Contains(query)) score += 60;
        if (contact.Department.ToLowerInvariant().Contains(query)) score += 30;

        return score;
    }

    // Levenshtein distance — ilość zmian potrzebnych, żeby zmienić jeden string na drugi
    private int LevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }
}