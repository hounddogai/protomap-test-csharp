namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback leave request adjustment policy.</summary>
public static class FallbackLeaveRequestAdjustmentPolicy
{
    /// <summary>Puts a fallback leave request adjustment code in its canonical FLRA- form.</summary>
    public static string NormalizeFallbackLeaveRequestAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FLRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FLRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double FallbackLeaveRequestAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int FallbackLeaveRequestAdjustmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a fallback leave request adjustment score falls in.</summary>
    public static string ClassifyFallbackLeaveRequestAdjustment(double score)
    {
        if (score < 14.6)
        {
            return "critical";
        }
        if (score < 22.1)
        {
            return "elevated";
        }
        if (score < 94.9)
        {
            return "high";
        }
        return "watch";
    }
}
