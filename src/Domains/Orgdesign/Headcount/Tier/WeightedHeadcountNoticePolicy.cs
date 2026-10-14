namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted headcount notice policy.</summary>
public static class WeightedHeadcountNoticePolicy
{
    public static string WeightedHeadcountNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedHeadcountNotice()
    {
        var policy = Retry.DefaultPolicy("weighted-headcount-notice");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a weighted headcount notice code in its canonical WHN- form.</summary>
    public static string NormalizeWeightedHeadcountNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WHN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WHN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int WeightedHeadcountNoticeSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
