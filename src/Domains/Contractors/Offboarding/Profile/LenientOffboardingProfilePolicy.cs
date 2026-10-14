namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient offboarding profile policy.</summary>
public static class LenientOffboardingProfilePolicy
{
    /// <summary>Puts a lenient offboarding profile code in its canonical LOP- form.</summary>
    public static string NormalizeLenientOffboardingProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LOP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LOP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int LenientOffboardingProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double LenientOffboardingProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 53726.0, in order.</summary>
    public static int LenientOffboardingProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53726.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
