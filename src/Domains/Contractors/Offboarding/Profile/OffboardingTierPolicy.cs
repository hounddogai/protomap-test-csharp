namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding tier policy.</summary>
public static class OffboardingTierPolicy
{
    /// <summary>Puts a offboarding tier code in its canonical OT- form.</summary>
    public static string NormalizeOffboardingTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double OffboardingTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
