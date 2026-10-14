namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

/// <summary>Functions of fallback certification bundle from profile.</summary>
public static class FallbackCertificationBundleFromProfile
{
    public static Dictionary<string, double> SummarizeLegacyExceptionProfilesForBundle(List<LegacyExceptionProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<FallbackCertificationBundle> DeriveFallbackCertificationBundlesFromProfile(List<LegacyExceptionProfile> items)
    {
        var outValue = new List<FallbackCertificationBundle>();
        foreach (var item in items)
        {
            var derived = FallbackCertificationBundle.NewFallbackCertificationBundle(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeFallbackCertificationBundle(FallbackCertificationBundle item)
    {
        return CappedExceptionCheckpointPolicy.ClassifyCappedExceptionCheckpoint(item.ProjectedVolume());
    }
}
