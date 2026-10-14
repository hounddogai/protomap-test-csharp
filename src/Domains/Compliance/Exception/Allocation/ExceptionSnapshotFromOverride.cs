namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Compliance.Certification.CertificationOverride;

/// <summary>Functions of exception snapshot from override.</summary>
public static class ExceptionSnapshotFromOverride
{
    public static Dictionary<string, double> SummarizeDefaultCertificationOverridesForSnapshot(List<DefaultCertificationOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<ExceptionSnapshot> DeriveExceptionSnapshotsFromOverride(List<DefaultCertificationOverride> items)
    {
        var outValue = new List<ExceptionSnapshot>();
        foreach (var item in items)
        {
            var derived = ExceptionSnapshot.NewExceptionSnapshot(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileExceptionSnapshot(ExceptionSnapshot item)
    {
        return CertificationTierPolicy.CertificationTierBucket(item.EffectiveExposure());
    }
}
