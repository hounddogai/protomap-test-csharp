namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional span draft policy.</summary>
public static class RegionalSpanDraftPolicy
{
    /// <summary>Puts a regional span draft code in its canonical RSD- form.</summary>
    public static string NormalizeRegionalSpanDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RSD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RSD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RegionalSpanDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalSpanDraft()
    {
        var policy = Retry.DefaultPolicy("regional-span-draft");
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
}
