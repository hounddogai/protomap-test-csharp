namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal policy ack override policy.</summary>
public static class SeasonalPolicyAckOverridePolicy
{
    /// <summary>Puts a seasonal policy ack override code in its canonical SPAO- form.</summary>
    public static string NormalizeSeasonalPolicyAckOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPAO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPAO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalPolicyAckOverride()
    {
        var policy = Retry.DefaultPolicy("seasonal-policy-ack-override");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string SeasonalPolicyAckOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
