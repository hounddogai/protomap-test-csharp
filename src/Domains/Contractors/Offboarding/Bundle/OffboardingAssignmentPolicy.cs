namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding assignment policy.</summary>
public static class OffboardingAssignmentPolicy
{
    /// <summary>Puts a offboarding assignment code in its canonical OA- form.</summary>
    public static string NormalizeOffboardingAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string OffboardingAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
