namespace Acme.Workforce.Contractors.Rate.Notice;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted rate notice policy.</summary>
public static class WeightedRateNoticePolicy
{
    /// <summary>Puts a weighted rate notice code in its canonical WRN- form.</summary>
    public static string NormalizeWeightedRateNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WRN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WRN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedRateNotice()
    {
        var policy = Retry.DefaultPolicy("weighted-rate-notice");
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

    public static string WeightedRateNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
