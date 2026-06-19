namespace DirectorySearchApi.Api.Models;

public class SearchQuery
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Query { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "api-user";
}