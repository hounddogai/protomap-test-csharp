namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of merit cycle exception policy.</summary>
public static class MeritCycleExceptionPolicy
{
    public static double MeritCycleExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.12;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 33265.0, in order.</summary>
    public static int MeritCycleExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33265.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a merit cycle exception code in its canonical MCE- form.</summary>
    public static string NormalizeMeritCycleExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string MeritCycleExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
