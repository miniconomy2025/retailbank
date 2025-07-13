using System.ComponentModel.DataAnnotations;

namespace RetailBank.Models.Dtos;

public record StartSimulationRequest(
    [property: Required]
    [property: Range(0, ulong.MaxValue)]
    ulong EpochStartTime
);
