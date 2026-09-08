using System;
using System.Collections.Generic;
using SelfShunt.API;

namespace SelfShunt;

public sealed class SelfShuntIntegrationApi : ISelfShuntIntegrationApi
{
    private readonly object gate = new object();
    private readonly HashSet<string> suspendedStations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> operations = new Dictionary<string, string>(StringComparer.Ordinal);
    private readonly Dictionary<string, JobContext> externalJobs = new Dictionary<string, JobContext>(StringComparer.Ordinal);
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
        EconomicAuthority.SetExternalAuthority(IsStrictEconomyActive);
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
        EconomicAuthority.SetExternalAuthority(IsStrictEconomyActive);
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

    public bool IsExternalEconomicAuthority => IsStrictEconomyActive;

    public bool TryRegisterExternalJob(SelfShuntExternalJobRegistration registration)
    {
        if (!IsHost || !IsStrictEconomyActive || registration == null ||
            string.IsNullOrWhiteSpace(registration.OperationId) || string.IsNullOrWhiteSpace(registration.JobId) ||
            string.IsNullOrWhiteSpace(registration.StationId) || string.IsNullOrWhiteSpace(registration.CargoId)) return false;
        var fingerprint = "external-job|" + registration.JobId + "|" + registration.StationId + "|" + registration.CargoId;
        lock (gate)
        {
            if (operations.TryGetValue(registration.OperationId, out var known)) return string.Equals(known, fingerprint, StringComparison.Ordinal);
            if (externalJobs.ContainsKey(registration.JobId)) return false;
            operations.Add(registration.OperationId, fingerprint);
            externalJobs.Add(registration.JobId, new JobContext
            {
                OperationId = registration.OperationId,
                JobId = registration.JobId,
                StationId = registration.StationId,
                CargoId = registration.CargoId
            });
        }
        PublishLifecycle(SelfShuntIntegrationEventType.ExternalJobCreated, ContextForExternal(registration.JobId), 0, "external-job-registered");
        Main.SelfShuntModEntry?.Logger.Log("Integration API: registered externally-authoritative job " + registration.JobId + ".");
        return true;
    }

    internal bool ShouldGenerateAt(string stationId) => !IsNewGenerationSuspended(stationId);
    internal bool ShouldPopulateNaturalCars => !IsNaturalCarPopulationSuspended;
    internal bool IsStrictEconomyActive
    {
        get { lock (gate) return naturalCarPopulationSuspended && suspendedStations.Contains("*"); }
    }

    internal void PublishLifecycle(SelfShuntIntegrationEventType type, JobContext context, decimal quantity = 0, string resultCode = "")
    {
        if (!IsHost || context == null) return;
        Publish(new SelfShuntIntegrationEvent
        {
            Type = type,
            OperationId = context.OperationId,
            JobId = context.JobId,
            StationId = context.StationId,
            CargoId = context.CargoId,
            CumulativeQuantity = quantity,
            ObservedPayout = 0,
            ResultCode = resultCode
        });
    }

    internal JobContext Correlate(JobContext context)
    {
        if (string.IsNullOrWhiteSpace(context.JobId)) return context;
        lock (gate) return externalJobs.TryGetValue(context.JobId, out var external) ? external : context;
    }

    internal void ForgetExternalJob(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId)) return;
        lock (gate) externalJobs.Remove(jobId);
    }

    private JobContext ContextForExternal(string jobId)
    {
        lock (gate) return externalJobs[jobId];
    }

    private void Publish(SelfShuntIntegrationEvent value)
    {
        try { EventPublished?.Invoke(value); }
        catch (Exception exception) { Main.SelfShuntModEntry?.Logger.Error("SelfShunt integration event subscriber failed: " + exception.Message); }
    }
}

internal sealed class JobContext
{
    public string OperationId { get; set; } = "";
    public string JobId { get; set; } = "";
    public string StationId { get; set; } = "";
    public string CargoId { get; set; } = "";
}

public static class SelfShuntApi
{
    public static ISelfShuntIntegrationApi Instance { get; } = new SelfShuntIntegrationApi();
    internal static SelfShuntIntegrationApi Runtime => (SelfShuntIntegrationApi)Instance;
}
