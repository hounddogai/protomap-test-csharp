namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of retro pay score policy.</summary>
public static class RetroPayScorePolicy
{
    /// <summary>Puts a retro pay score code in its canonical RPS- form.</summary>
    public static string NormalizeRetroPayScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RetroPayScoreSpan(int start, int end)
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
        return span / 86400;
    }

    public static string RetroPayScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRetroPayScore()
    {
        var policy = Retry.DefaultPolicy("retro-pay-score");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
