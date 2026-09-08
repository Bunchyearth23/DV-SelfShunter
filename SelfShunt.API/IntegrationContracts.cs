using System;

namespace SelfShunt.API;

public enum SelfShuntIntegrationEventType
{
    GenerationPolicyChanged,
    NaturalCarPopulationPolicyChanged,
    ExternalJobCreated,
    LoadingObserved,
    DeliveryObserved,
    Completed,
    Cancelled,
    Expired
}

public sealed class SelfShuntIntegrationEvent
{
    public int SchemaVersion { get; set; } = 1;
    public SelfShuntIntegrationEventType Type { get; set; }
    public string OperationId { get; set; } = "";
    public string JobId { get; set; } = "";
    public string StationId { get; set; } = "";
    public string CargoId { get; set; } = "";
    public decimal CumulativeQuantity { get; set; }
    public string ResultCode { get; set; } = "";
}

public interface ISelfShuntIntegrationApi
{
    int ApiVersion { get; }
    bool IsHost { get; }
    bool CanControlNewGeneration { get; }
    bool CanControlNaturalCarPopulation { get; }
    bool SetNewGenerationSuspended(string operationId, string stationId, bool suspended);
    bool IsNewGenerationSuspended(string stationId);
    bool SetNaturalCarPopulationSuspended(string operationId, bool suspended);
    bool IsNaturalCarPopulationSuspended { get; }
    event Action<SelfShuntIntegrationEvent>? EventPublished;
}
