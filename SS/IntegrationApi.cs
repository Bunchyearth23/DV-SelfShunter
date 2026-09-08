using System;
using System.Collections.Generic;
using SelfShunt.API;

namespace SelfShunt;

public sealed class SelfShuntIntegrationApi : ISelfShuntIntegrationApi
{
    private readonly object gate = new object();
    private readonly HashSet<string> suspendedStations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> operations = new Dictionary<string, string>(StringComparer.Ordinal);
    private bool naturalCarPopulationSuspended;

    public int ApiVersion => 1;
    public bool IsHost => MultiplayerShim.IsHost;
    public bool CanControlNewGeneration => true;
    public bool CanControlNaturalCarPopulation => true;
    public event Action<SelfShuntIntegrationEvent>? EventPublished;

    public bool SetNewGenerationSuspended(string operationId, string stationId, bool suspended)
    {
        if (!IsHost || string.IsNullOrWhiteSpace(operationId)) return false;
        var scope = string.IsNullOrWhiteSpace(stationId) ? "*" : stationId.Trim();
        var fingerprint = scope + "|" + suspended;
        lock (gate)
        {
            if (operations.TryGetValue(operationId, out var known)) return string.Equals(known, fingerprint, StringComparison.Ordinal);
            operations.Add(operationId, fingerprint);
            if (suspended) suspendedStations.Add(scope); else suspendedStations.Remove(scope);
        }
        Publish(new SelfShuntIntegrationEvent
        {
            Type = SelfShuntIntegrationEventType.GenerationPolicyChanged,
            OperationId = operationId,
            StationId = scope,
            ResultCode = suspended ? "new-generation-suspended" : "new-generation-enabled"
        });
        Main.SelfShuntModEntry?.Logger.Log("Integration API: " + scope + " " + (suspended ? "new job generation suspended." : "new job generation enabled."));
        return true;
    }

    public bool IsNewGenerationSuspended(string stationId)
    {
        lock (gate) return suspendedStations.Contains("*") || (!string.IsNullOrWhiteSpace(stationId) && suspendedStations.Contains(stationId));
    }

    public bool SetNaturalCarPopulationSuspended(string operationId, bool suspended)
    {
        if (!IsHost || string.IsNullOrWhiteSpace(operationId)) return false;
        var fingerprint = "natural-car-population|" + suspended;
        lock (gate)
        {
            if (operations.TryGetValue(operationId, out var known)) return string.Equals(known, fingerprint, StringComparison.Ordinal);
            operations.Add(operationId, fingerprint);
            naturalCarPopulationSuspended = suspended;
        }
        Publish(new SelfShuntIntegrationEvent
        {
            Type = SelfShuntIntegrationEventType.NaturalCarPopulationPolicyChanged,
            OperationId = operationId,
            ResultCode = suspended ? "natural-car-population-suspended" : "natural-car-population-enabled"
        });
        Main.SelfShuntModEntry?.Logger.Log("Integration API: natural car population " + (suspended ? "suspended." : "enabled."));
        return true;
    }

    public bool IsNaturalCarPopulationSuspended
    {
        get { lock (gate) return naturalCarPopulationSuspended; }
    }

    internal bool ShouldGenerateAt(string stationId) => !IsNewGenerationSuspended(stationId);
    internal bool ShouldPopulateNaturalCars => !IsNaturalCarPopulationSuspended;

    private void Publish(SelfShuntIntegrationEvent value)
    {
        try { EventPublished?.Invoke(value); }
        catch (Exception exception) { Main.SelfShuntModEntry?.Logger.Error("SelfShunt integration event subscriber failed: " + exception.Message); }
    }
}

public static class SelfShuntApi
{
    public static ISelfShuntIntegrationApi Instance { get; } = new SelfShuntIntegrationApi();
    internal static SelfShuntIntegrationApi Runtime => (SelfShuntIntegrationApi)Instance;
}
