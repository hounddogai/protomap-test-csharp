namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding segment policy.</summary>
public static class OffboardingSegmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 93372.0, in order.</summary>
    public static int OffboardingSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93372.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a offboarding segment code in its canonical OS- form.</summary>
    public static string NormalizeOffboardingSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a offboarding segment score falls in.</summary>
    public static string ClassifyOffboardingSegment(double score)
    {
        if (score < 33.9)
        {
            return "critical";
        }
        if (score < 65.0)
        {
            return "elevated";
        }
        return "ok";
    }

    public static string OffboardingSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
