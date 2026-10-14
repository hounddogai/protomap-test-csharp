namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy ack request policy.</summary>
public static class PolicyAckRequestPolicy
{
    public static List<string> MergePolicyAckRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double PolicyAckRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.03;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a policy ack request code in its canonical PAR- form.</summary>
    public static string NormalizePolicyAckRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
