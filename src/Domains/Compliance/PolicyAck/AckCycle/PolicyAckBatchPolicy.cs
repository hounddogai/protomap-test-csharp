namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy ack batch policy.</summary>
public static class PolicyAckBatchPolicy
{
    /// <summary>Puts a policy ack batch code in its canonical PAB- form.</summary>
    public static string NormalizePolicyAckBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PolicyAckBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergePolicyAckBatchLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
