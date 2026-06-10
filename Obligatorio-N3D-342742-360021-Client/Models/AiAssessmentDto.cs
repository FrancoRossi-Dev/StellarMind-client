using System.Text.Json.Serialization;

namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class AiAssessmentDto
    {
        [JsonPropertyName("indicador")]
        public string? Indicator { get; set; }

        [JsonPropertyName("detalle")]
        public string? Detail { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
