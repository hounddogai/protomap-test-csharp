namespace Acme.Workforce.Orgdesign.Span.Digest;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of span digest policy.</summary>
public static class SpanDigestPolicy
{
    public static double SpanDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a span digest code in its canonical SD- form.</summary>
    public static string NormalizeSpanDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int SpanDigestSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSpanDigest()
    {
        var policy = Retry.DefaultPolicy("span-digest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
