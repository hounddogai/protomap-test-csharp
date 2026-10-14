namespace Acme.Workforce.Contractors.Offboarding.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding target policy.</summary>
public static class OffboardingTargetPolicy
{
    public static string OffboardingTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a offboarding target code in its canonical OT- form.</summary>
    public static string NormalizeOffboardingTargetCode(string code)
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
}
