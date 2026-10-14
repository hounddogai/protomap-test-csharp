namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback policy score policy.</summary>
public static class FallbackPolicyScorePolicy
{
    /// <summary>Puts a fallback policy score code in its canonical FPS- form.</summary>
    public static string NormalizeFallbackPolicyScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double FallbackPolicyScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.03;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 7054.0, in order.</summary>
    public static int FallbackPolicyScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7054.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string FallbackPolicyScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
