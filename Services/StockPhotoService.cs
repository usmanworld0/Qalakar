using Microsoft.JSInterop;
using System.Text.Json;

namespace Qalakar.Services;

public class StockPhotoService
{
    private readonly IJSRuntime _jsRuntime;

    public StockPhotoService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<List<StockPhotoModel>> GetStockPhotosAsync()
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.getStockPhotos");
            var photos = new List<StockPhotoModel>();

            if (result.GetProperty("success").GetBoolean() && result.TryGetProperty("data", out var dataElement))
            {
                foreach (var item in dataElement.EnumerateArray())
                {
                    photos.Add(new StockPhotoModel
                    {
                        Id = item.GetProperty("id").GetString() ?? "",
                        Title = item.GetProperty("title").GetString() ?? "",
                        Description = item.GetProperty("description").GetString() ?? "",
                        Category = item.GetProperty("category").GetString() ?? "",
                        Tags = item.TryGetProperty("tags", out var tagsElement) ? 
                               tagsElement.EnumerateArray().Select(t => t.GetString() ?? "").ToList() : 
                               new List<string>(),
                        ImageUrl = item.TryGetProperty("imageUrl", out var imgElement) ? imgElement.GetString() ?? "" : "",
                        UploadedBy = item.GetProperty("uploadedBy").GetString() ?? "",
                        UploadedByEmail = item.GetProperty("uploadedByEmail").GetString() ?? "",
                        Downloads = item.TryGetProperty("downloads", out var downloadsElement) ? downloadsElement.GetInt32() : 0,
                        CreatedAt = DateTime.TryParse(item.GetProperty("createdAt").GetString(), out var createdAt) ? createdAt : DateTime.Now
                    });
                }
            }

            return photos;
        }
        catch (Exception)
        {
            return new List<StockPhotoModel>();
        }
    }

    public async Task<List<StockPhotoModel>> GetUserStockPhotosAsync(string userId)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.getUserStockPhotos", userId);
            var photos = new List<StockPhotoModel>();

            if (result.GetProperty("success").GetBoolean() && result.TryGetProperty("data", out var dataElement))
            {
                foreach (var item in dataElement.EnumerateArray())
                {
                    photos.Add(new StockPhotoModel
                    {
                        Id = item.GetProperty("id").GetString() ?? "",
                        Title = item.GetProperty("title").GetString() ?? "",
                        Description = item.GetProperty("description").GetString() ?? "",
                        Category = item.GetProperty("category").GetString() ?? "",
                        Tags = item.TryGetProperty("tags", out var tagsElement) ? 
                               tagsElement.EnumerateArray().Select(t => t.GetString() ?? "").ToList() : 
                               new List<string>(),
                        ImageUrl = item.TryGetProperty("imageUrl", out var imgElement) ? imgElement.GetString() ?? "" : "",
                        UploadedBy = item.GetProperty("uploadedBy").GetString() ?? "",
                        UploadedByEmail = item.GetProperty("uploadedByEmail").GetString() ?? "",
                        Downloads = item.TryGetProperty("downloads", out var downloadsElement) ? downloadsElement.GetInt32() : 0,
                        CreatedAt = DateTime.TryParse(item.GetProperty("createdAt").GetString(), out var createdAt) ? createdAt : DateTime.Now
                    });
                }
            }

            return photos;
        }
        catch (Exception)
        {
            return new List<StockPhotoModel>();
        }
    }

    public async Task<ServiceResult> AddStockPhotoAsync(object photoData)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.addStockPhoto", photoData);
            return new ServiceResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? ""
            };
        }
        catch (Exception ex)
        {
            return new ServiceResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResult> UpdateStockPhotoAsync(string photoId, object updateData)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.updateStockPhoto", photoId, updateData);
            return new ServiceResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? ""
            };
        }
        catch (Exception ex)
        {
            return new ServiceResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResult> DeleteStockPhotoAsync(string photoId)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.deleteStockPhoto", photoId);
            return new ServiceResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? ""
            };
        }
        catch (Exception ex)
        {
            return new ServiceResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<ServiceResult> IncrementDownloadAsync(string photoId)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<JsonElement>("firebaseFirestore.incrementDownload", photoId);
            return new ServiceResult
            {
                Success = result.GetProperty("success").GetBoolean(),
                Message = result.GetProperty("message").GetString() ?? ""
            };
        }
        catch (Exception ex)
        {
            return new ServiceResult { Success = false, Message = ex.Message };
        }
    }
}

public class StockPhotoModel
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public string ImageUrl { get; set; } = "";
    public string UploadedBy { get; set; } = "";
    public string UploadedByEmail { get; set; } = "";
    public int Downloads { get; set; }
    public DateTime CreatedAt { get; set; }
}
