namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Functions of seasonal reorg exception from bundle.</summary>
public static class SeasonalReorgExceptionFromBundle
{
    public static Dictionary<string, double> SummarizeHeadcountBundlesForException(List<HeadcountBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<SeasonalReorgException> DeriveSeasonalReorgExceptionsFromBundle(List<HeadcountBundle> items)
    {
        var outValue = new List<SeasonalReorgException>();
        foreach (var item in items)
        {
            var derived = SeasonalReorgException.NewSeasonalReorgException(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double NormalizeSeasonalReorgException(SeasonalReorgException item)
    {
        return HeadcountBundlePolicy.HeadcountBundleWeight(6, true);
    }
}
