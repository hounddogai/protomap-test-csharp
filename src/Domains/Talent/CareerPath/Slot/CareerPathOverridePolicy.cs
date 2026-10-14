namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of career path override policy.</summary>
public static class CareerPathOverridePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 72938.0, in order.</summary>
    public static int CareerPathOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72938.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int CareerPathOverrideSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static string CareerPathOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a career path override code in its canonical CPO- form.</summary>
    public static string NormalizeCareerPathOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
