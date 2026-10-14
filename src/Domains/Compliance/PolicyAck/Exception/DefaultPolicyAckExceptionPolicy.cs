namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default policy ack exception policy.</summary>
public static class DefaultPolicyAckExceptionPolicy
{
    public static int DefaultPolicyAckExceptionSpan(int start, int end)
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

    public static List<string> MergeDefaultPolicyAckExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a default policy ack exception score falls in.</summary>
    public static string ClassifyDefaultPolicyAckException(double score)
    {
        if (score < 1.8)
        {
            return "elevated";
        }
        if (score < 24.0)
        {
            return "ok";
        }
        if (score < 36.9)
        {
            return "high";
        }
        return "medium";
    }

    public static string DefaultPolicyAckExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
