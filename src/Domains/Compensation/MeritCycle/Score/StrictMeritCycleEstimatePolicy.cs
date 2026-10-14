namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict merit cycle estimate policy.</summary>
public static class StrictMeritCycleEstimatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 1552.0, in order.</summary>
    public static int StrictMeritCycleEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1552.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int StrictMeritCycleEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    public static double StrictMeritCycleEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a strict merit cycle estimate code in its canonical SMCE- form.</summary>
    public static string NormalizeStrictMeritCycleEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SMCE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SMCE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
