namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of policy ack window policy.</summary>
public static class PolicyAckWindowPolicy
{
    public static double PolicyAckWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergePolicyAckWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int PolicyAckWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a policy ack window code in its canonical PAW- form.</summary>
    public static string NormalizePolicyAckWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
