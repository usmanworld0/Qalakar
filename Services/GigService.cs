using Microsoft.JSInterop;
using System.Text.Json;

namespace Qalakar.Services;

public class GigService
{
    private readonly IJSRuntime _jsRuntime;

    public GigService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<OperationResult> CreateGigAsync(object gigData)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.addGig", gigData);
            return new OperationResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? "",
                Data = result.TryGetProperty("id", out var idElement) ? idElement.GetString() : null
            };
        }
        catch (Exception ex)
        {
            return new OperationResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<List<GigModel>> GetGigsAsync()
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.getGigs");
            var gigs = new List<GigModel>();

            if (result.GetProperty("success").GetBoolean() && result.TryGetProperty("data", out var dataElement))
            {
                foreach (var item in dataElement.EnumerateArray())
                {
                    var dates = new List<DateTime>();
                    if (item.TryGetProperty("availableDates", out var datesElement))
                    {
                        foreach (var dateItem in datesElement.EnumerateArray())
                        {
                            if (DateTime.TryParse(dateItem.GetString(), out var date))
                            {
                                dates.Add(date);
                            }
                        }
                    }

                    gigs.Add(new GigModel
                    {
                        Id = item.GetProperty("id").GetString() ?? "",
                        Title = item.GetProperty("title").GetString() ?? "",
                        Description = item.GetProperty("description").GetString() ?? "",
                        Budget = item.TryGetProperty("budget", out var budgetElement) ? budgetElement.GetDecimal() : 0,
                        Location = item.GetProperty("location").GetString() ?? "",
                        Category = item.GetProperty("category").GetString() ?? "",
                        ImageUrl = item.TryGetProperty("imageUrl", out var imgElement) ? 
                                  (imgElement.ValueKind == JsonValueKind.String ? imgElement.GetString() ?? "" : "") : "",
                        Dates = dates,
                        CreatedBy = item.GetProperty("createdBy").GetString() ?? "",
                        CreatedByEmail = item.GetProperty("createdByEmail").GetString() ?? "",
                        CreatedAt = DateTime.TryParse(item.GetProperty("createdAt").GetString(), out var createdAt) ? createdAt : DateTime.Now
                    });
                }
            }

            return gigs;
        }
        catch (Exception)
        {
            return new List<GigModel>();
        }
    }

    public async Task<List<GigModel>> GetUserGigsAsync(string userId)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.getUserGigs", userId);
            var gigs = new List<GigModel>();

            if (result.GetProperty("success").GetBoolean() && result.TryGetProperty("data", out var dataElement))
            {
                foreach (var item in dataElement.EnumerateArray())
                {
                    var dates = new List<DateTime>();
                    if (item.TryGetProperty("availableDates", out var datesElement))
                    {
                        foreach (var dateItem in datesElement.EnumerateArray())
                        {
                            if (DateTime.TryParse(dateItem.GetString(), out var date))
                            {
                                dates.Add(date);
                            }
                        }
                    }

                    gigs.Add(new GigModel
                    {
                        Id = item.GetProperty("id").GetString() ?? "",
                        Title = item.GetProperty("title").GetString() ?? "",
                        Description = item.GetProperty("description").GetString() ?? "",
                        Budget = item.TryGetProperty("budget", out var budgetElement) ? budgetElement.GetDecimal() : 0,
                        Location = item.GetProperty("location").GetString() ?? "",
                        Category = item.GetProperty("category").GetString() ?? "",
                        ImageUrl = item.TryGetProperty("imageUrl", out var imgElement) ? 
                                  (imgElement.ValueKind == JsonValueKind.String ? imgElement.GetString() ?? "" : "") : "",
                        Dates = dates,
                        CreatedBy = item.GetProperty("createdBy").GetString() ?? "",
                        CreatedByEmail = item.GetProperty("createdByEmail").GetString() ?? "",
                        CreatedAt = DateTime.TryParse(item.GetProperty("createdAt").GetString(), out var createdAt) ? createdAt : DateTime.Now
                    });
                }
            }

            return gigs;
        }
        catch (Exception)
        {
            return new List<GigModel>();
        }
    }

    public async Task<OperationResult> UpdateGigAsync(string gigId, object updateData)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.updateGig", gigId, updateData);
            return new OperationResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? ""
            };
        }
        catch (Exception ex)
        {
            return new OperationResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<OperationResult> DeleteGigAsync(string gigId, string userId)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.deleteGig", gigId, userId);
            return new OperationResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? ""
            };
        }
        catch (Exception ex)
        {
            return new OperationResult { Success = false, Message = ex.Message };
        }
    }
}

public class GigModel
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Budget { get; set; }
    public string Location { get; set; } = "";
    public string Category { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public List<DateTime> Dates { get; set; } = new();
    public string CreatedBy { get; set; } = "";
    public string CreatedByEmail { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class OperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public object? Data { get; set; }
}
