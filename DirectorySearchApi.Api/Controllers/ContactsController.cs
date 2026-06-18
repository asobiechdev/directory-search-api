using DirectorySearchApi.Api.Models;
using DirectorySearchApi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DirectorySearchApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IContactSearchService _searchService;

    public ContactsController(IContactSearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Search contacts by name, phone number, or department.
    /// Supports fuzzy matching (typo tolerance).
    /// </summary>
    /// <param name="query">Search term (firstName, lastName, phoneNumber, department)</param>
    /// <returns>List of matching contacts, ordered by relevance</returns>
    [HttpGet("search")]
    public ActionResult<List<Contact>> Search([FromQuery] string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { error = "Query parameter is required" });

        var results = _searchService.Search(query);
        return Ok(results);
    }

    /// <summary>
    /// Get all contacts.
    /// </summary>
    [HttpGet]
    public ActionResult<List<Contact>> GetAll()
    {
        var results = _searchService.Search("");
        return Ok(results);
    }
}