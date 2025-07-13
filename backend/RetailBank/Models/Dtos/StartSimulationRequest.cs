using System.ComponentModel.DataAnnotations;

namespace RetailBank.Models.Dtos;

public record StartSimulationRequest(
    [property: Required]
    [property: Range(1, ulong.MaxValue)]
    ulong EpochStartTime
);
