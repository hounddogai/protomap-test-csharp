namespace Acme.Workforce.Compliance.Exception.Cycle;

using Acme.Workforce.Compliance.Certification.Digest;

/// <summary>Functions of exception threshold from digest.</summary>
public static class ExceptionThresholdFromDigest
{
    public static Dictionary<string, double> SummarizeSeasonalCertificationDigestsForThreshold(List<SeasonalCertificationDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<ExceptionThreshold> DeriveExceptionThresholdsFromDigest(List<SeasonalCertificationDigest> items)
    {
        var outValue = new List<ExceptionThreshold>();
        foreach (var item in items)
        {
            var derived = ExceptionThreshold.NewExceptionThreshold(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchExceptionThreshold(ExceptionThreshold item)
    {
        return ManualCertificationClaimPolicy.ManualCertificationClaimWeight(3, true);
    }
}
