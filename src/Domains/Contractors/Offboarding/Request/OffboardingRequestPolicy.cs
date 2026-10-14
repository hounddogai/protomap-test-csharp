namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding request policy.</summary>
public static class OffboardingRequestPolicy
{
    /// <summary>Puts a offboarding request code in its canonical OR- form.</summary>
    public static string NormalizeOffboardingRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string OffboardingRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double OffboardingRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.01;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
