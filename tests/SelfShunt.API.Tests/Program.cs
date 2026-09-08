using SelfShunt.API;

var failures = new List<string>();
Check((int)SelfShuntIntegrationEventType.GenerationPolicyChanged == 0, "existing event values must remain stable");
Check((int)SelfShuntIntegrationEventType.Expired == 7, "v1 terminal event values must remain stable");
Check((int)SelfShuntIntegrationEventType.JobCreated == 8, "JobCreated must be append-only");

var value = new SelfShuntIntegrationEvent();
Check(value.SchemaVersion == 1, "event schema must default to v1");
Check(value.OperationId != null && value.JobId != null && value.StationId != null && value.CargoId != null && value.ResultCode != null,
    "event string members must be safe for reflection consumers");

var api = typeof(ISelfShuntIntegrationApi);
Check(api.GetProperty(nameof(ISelfShuntIntegrationApi.ApiVersion)) != null, "API version property is required");
Check(api.GetMethod(nameof(ISelfShuntIntegrationApi.SetNewGenerationSuspended)) != null, "generation control is required");
Check(api.GetMethod(nameof(ISelfShuntIntegrationApi.SetNaturalCarPopulationSuspended)) != null, "population control is required");
Check(api.GetProperty(nameof(ISelfShuntIntegrationApi.IsExternalEconomicAuthority)) != null, "external economic authority evidence is required");
Check(api.GetMethod(nameof(ISelfShuntIntegrationApi.TryRegisterExternalJob)) != null, "external job correlation is required");
Check(api.GetEvent(nameof(ISelfShuntIntegrationApi.EventPublished)) != null, "lifecycle event surface is required");

if (failures.Count != 0)
{
    foreach (var failure in failures) Console.Error.WriteLine("FAIL: " + failure);
    return 1;
}

Console.WriteLine("SelfShunt.API contract tests: 11/11 passed");
return 0;

void Check(bool condition, string message)
{
    if (!condition) failures.Add(message);
}
