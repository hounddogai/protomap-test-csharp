namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy ack item policy.</summary>
public static class PolicyAckItemPolicy
{
    /// <summary>Puts a policy ack item code in its canonical PAI- form.</summary>
    public static string NormalizePolicyAckItemCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAI-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAI-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePolicyAckItemLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int PolicyAckItemSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
