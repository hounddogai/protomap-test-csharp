namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global policy ack cycle policy.</summary>
public static class GlobalPolicyAckCyclePolicy
{
    public static double GlobalPolicyAckCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a global policy ack cycle code in its canonical GPAC- form.</summary>
    public static string NormalizeGlobalPolicyAckCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GPAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GPAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeGlobalPolicyAckCycleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a global policy ack cycle score falls in.</summary>
    public static string ClassifyGlobalPolicyAckCycle(double score)
    {
        if (score < 1.0)
        {
            return "elevated";
        }
        if (score < 82.7)
        {
            return "high";
        }
        return "minimal";
    }
}
