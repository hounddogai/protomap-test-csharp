namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Compliance.Certification.Slot;

/// <summary>Functions of exception target from metric.</summary>
public static class ExceptionTargetFromMetric
{
    public static Dictionary<string, double> SummarizeCertificationMetricsForTarget(List<CertificationMetric> items)
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

    public static List<ExceptionTarget> DeriveExceptionTargetsFromMetric(List<CertificationMetric> items)
    {
        var outValue = new List<ExceptionTarget>();
        foreach (var item in items)
        {
            var derived = ExceptionTarget.NewExceptionTarget(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AssignExceptionTarget(ExceptionTarget item)
    {
        return SeasonalCertificationManifestPolicy.SeasonalCertificationManifestWeight(7, true);
    }
}
