namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped floor plan threshold policy.</summary>
public static class CappedFloorPlanThresholdPolicy
{
    public static List<string> MergeCappedFloorPlanThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedFloorPlanThreshold()
    {
        var policy = Retry.DefaultPolicy("capped-floor-plan-threshold");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a capped floor plan threshold code in its canonical CFPT- form.</summary>
    public static string NormalizeCappedFloorPlanThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CFPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CFPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
