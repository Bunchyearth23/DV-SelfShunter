using System;
using System.Collections.Generic;
using DV.Logic.Job;
using HarmonyLib;

namespace SelfShunt;

internal static class EconomicAuthority
{
    private static readonly object Gate = new object();
    private static readonly Dictionary<Job, float> OriginalWages = new Dictionary<Job, float>();
    private static readonly System.Reflection.FieldInfo InitialWage =
        AccessTools.Field(typeof(Job), "initialWage") ?? throw new MissingFieldException(typeof(Job).FullName, "initialWage");
    private static bool externalAuthority;

    internal static void Track(Job job)
    {
        if (job == null) return;
        lock (Gate)
        {
            if (!OriginalWages.ContainsKey(job)) OriginalWages.Add(job, (float)InitialWage.GetValue(job));
            if (externalAuthority) InitialWage.SetValue(job, 0f);
        }
    }

    internal static void Untrack(Job job)
    {
        if (job == null) return;
        lock (Gate) OriginalWages.Remove(job);
    }

    internal static void SetExternalAuthority(bool enabled)
    {
        lock (Gate)
        {
            if (externalAuthority == enabled) return;
            externalAuthority = enabled;
            foreach (var pair in OriginalWages)
                InitialWage.SetValue(pair.Key, enabled ? 0f : pair.Value);
        }
        Main.SelfShuntModEntry?.Logger.Log(enabled
            ? "SelfShunt strict economy active: vanilla wages suppressed; BDVM is authoritative."
            : "SelfShunt strict economy inactive: original vanilla wages restored.");
    }
}
