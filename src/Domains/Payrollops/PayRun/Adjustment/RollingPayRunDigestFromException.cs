namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

/// <summary>Functions of rolling pay run digest from exception.</summary>
public static class RollingPayRunDigestFromException
{
    public static Dictionary<string, double> SummarizeAdaptiveGarnishmentExceptionsForDigest(List<AdaptiveGarnishmentException> items)
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

    public static List<RollingPayRunDigest> DeriveRollingPayRunDigestsFromException(List<AdaptiveGarnishmentException> items)
    {
        var outValue = new List<RollingPayRunDigest>();
        foreach (var item in items)
        {
            var derived = RollingPayRunDigest.NewRollingPayRunDigest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreRollingPayRunDigest(RollingPayRunDigest item)
    {
        return RegionalGarnishmentAgreementPolicy.RegionalGarnishmentAgreementBucket(item.AdjustedValue());
    }
}
