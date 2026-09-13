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
var registration = new SelfShuntExternalJobRegistration { DisplayReward = 1234, DisplayName = "Independent · SM → GF · Steel Billets" };
Check(registration.DisplayReward == 1234, "external jobs require a separate display-only reward");
Check(registration.DisplayName.IndexOf("Independent", StringComparison.Ordinal) >= 0, "external jobs expose a bounded display name");

if (failures.Count != 0)
{
    foreach (var failure in failures) Console.Error.WriteLine("FAIL: " + failure);
    return 1;
}

Console.WriteLine("SelfShunt.API contract tests: 13/13 passed");
return 0;

void Check(bool condition, string message)
{
    if (!condition) failures.Add(message);
}
