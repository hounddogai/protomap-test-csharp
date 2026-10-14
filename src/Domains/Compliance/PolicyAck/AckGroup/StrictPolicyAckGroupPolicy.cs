namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

/// <summary>Functions of strict policy ack group policy.</summary>
public static class StrictPolicyAckGroupPolicy
{
    public static int StrictPolicyAckGroupSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a strict policy ack group score falls in.</summary>
    public static string ClassifyStrictPolicyAckGroup(double score)
    {
        if (score < 31.0)
        {
            return "minimal";
        }
        if (score < 47.4)
        {
            return "low";
        }
        return "watch";
    }

    /// <summary>Puts a strict policy ack group code in its canonical SPAG- form.</summary>
    public static string NormalizeStrictPolicyAckGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPAG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPAG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
