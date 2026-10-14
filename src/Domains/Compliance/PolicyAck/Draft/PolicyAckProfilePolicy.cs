namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy ack profile policy.</summary>
public static class PolicyAckProfilePolicy
{
    public static double PolicyAckProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.06;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a policy ack profile code in its canonical PAP- form.</summary>
    public static string NormalizePolicyAckProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PolicyAckProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a policy ack profile score falls in.</summary>
    public static string ClassifyPolicyAckProfile(double score)
    {
        if (score < 37.5)
        {
            return "watch";
        }
        if (score < 44.9)
        {
            return "ok";
        }
        if (score < 74.6)
        {
            return "elevated";
        }
        return "low";
    }
}
