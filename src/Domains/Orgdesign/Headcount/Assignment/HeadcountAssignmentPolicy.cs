namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of headcount assignment policy.</summary>
public static class HeadcountAssignmentPolicy
{
    public static double HeadcountAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a headcount assignment code in its canonical HA- form.</summary>
    public static string NormalizeHeadcountAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 29124.0, in order.</summary>
    public static int HeadcountAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29124.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string HeadcountAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
