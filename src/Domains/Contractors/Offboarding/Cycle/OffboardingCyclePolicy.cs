namespace Acme.Workforce.Contractors.Offboarding.Cycle;

/// <summary>Functions of offboarding cycle policy.</summary>
public static class OffboardingCyclePolicy
{
    /// <summary>Puts a offboarding cycle code in its canonical OC- form.</summary>
    public static string NormalizeOffboardingCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a offboarding cycle score falls in.</summary>
    public static string ClassifyOffboardingCycle(double score)
    {
        if (score < 56.0)
        {
            return "critical";
        }
        if (score < 67.5)
        {
            return "high";
        }
        if (score < 83.1)
        {
            return "ok";
        }
        if (score < 97.1)
        {
            return "minimal";
        }
        return "elevated";
    }
}
