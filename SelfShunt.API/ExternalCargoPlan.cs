using System.Collections.Generic;

namespace SelfShunt.API;

/// <summary>Checks an externally assigned loading plan without inspecting Unity objects.</summary>
public static class ExternalCargoPlan
{
    public static bool IsQuantitySatisfied(float planned, float loaded) =>
        !float.IsNaN(planned) && !float.IsInfinity(planned) && planned >= 0f &&
        !float.IsNaN(loaded) && !float.IsInfinity(loaded) && loaded >= 0f &&
        System.Math.Abs(loaded - planned) <= 0.01f;
    public static bool IsSatisfied(IReadOnlyList<float>? planned, IReadOnlyList<float>? loaded, bool matchingCargo)
    {
        if (!matchingCargo || planned == null || loaded == null || planned.Count == 0 || planned.Count != loaded.Count) return false;
        for (var i = 0; i < planned.Count; i++)
            if (!IsQuantitySatisfied(planned[i], loaded[i])) return false;
        return true;
    }
}
