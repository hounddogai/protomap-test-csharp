namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Functions of legacy rate variance from exception.</summary>
public static class LegacyRateVarianceFromException
{
    public static Dictionary<string, double> SummarizeEngagementExceptionsForVariance(List<EngagementException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<LegacyRateVariance> DeriveLegacyRateVariancesFromException(List<EngagementException> items)
    {
        var outValue = new List<LegacyRateVariance>();
        foreach (var item in items)
        {
            var derived = LegacyRateVariance.NewLegacyRateVariance(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateLegacyRateVariance(LegacyRateVariance item)
    {
        return PrimaryEngagementProfilePolicy.PrimaryEngagementProfileWeight(3, true);
    }
}
