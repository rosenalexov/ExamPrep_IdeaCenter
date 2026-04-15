using System.Text.Json.Serialization;

namespace ExamPrep_IdeaCenter.DTOs;

internal class ApiResponseDto
{
    [JsonPropertyName("msg")]
    public string? Msg { get; set; }
    
    [JsonPropertyName("id")]
    public string? IdeaId { get; set; }
}