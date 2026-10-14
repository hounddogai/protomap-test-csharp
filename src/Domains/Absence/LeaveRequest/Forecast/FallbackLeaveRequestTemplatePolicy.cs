namespace Acme.Workforce.Absence.LeaveRequest.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback leave request template policy.</summary>
public static class FallbackLeaveRequestTemplatePolicy
{
    public static double FallbackLeaveRequestTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.39;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a fallback leave request template code in its canonical FLRT- form.</summary>
    public static string NormalizeFallbackLeaveRequestTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FLRT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FLRT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
