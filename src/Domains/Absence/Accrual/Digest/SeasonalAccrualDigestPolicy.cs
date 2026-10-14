namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal accrual digest policy.</summary>
public static class SeasonalAccrualDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 2329.0, in order.</summary>
    public static int SeasonalAccrualDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 2329.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeSeasonalAccrualDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string SeasonalAccrualDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a seasonal accrual digest score falls in.</summary>
    public static string ClassifySeasonalAccrualDigest(double score)
    {
        if (score < 16.7)
        {
            return "medium";
        }
        if (score < 42.5)
        {
            return "low";
        }
        if (score < 85.9)
        {
            return "high";
        }
        return "critical";
    }
}
