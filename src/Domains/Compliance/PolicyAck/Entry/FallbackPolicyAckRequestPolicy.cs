namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback policy ack request policy.</summary>
public static class FallbackPolicyAckRequestPolicy
{
    public static string FallbackPolicyAckRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int FallbackPolicyAckRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static double FallbackPolicyAckRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.55;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
