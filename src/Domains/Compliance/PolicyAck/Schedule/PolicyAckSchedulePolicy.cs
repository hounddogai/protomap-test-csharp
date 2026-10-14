namespace Acme.Workforce.Compliance.PolicyAck.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of policy ack schedule policy.</summary>
public static class PolicyAckSchedulePolicy
{
    public static string PolicyAckScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static int PolicyAckScheduleSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a policy ack schedule score falls in.</summary>
    public static string ClassifyPolicyAckSchedule(double score)
    {
        if (score < 4.8)
        {
            return "minimal";
        }
        if (score < 70.5)
        {
            return "medium";
        }
        if (score < 90.2)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Puts a policy ack schedule code in its canonical PAS- form.</summary>
    public static string NormalizePolicyAckScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
