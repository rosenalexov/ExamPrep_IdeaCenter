using System.Text.Json.Serialization;

namespace ExamPrep_IdeaCenter.DTOs;

internal class IdeaDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}