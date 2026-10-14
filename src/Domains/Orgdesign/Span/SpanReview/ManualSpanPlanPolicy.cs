namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual span plan policy.</summary>
public static class ManualSpanPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualSpanPlan()
    {
        var policy = Retry.DefaultPolicy("manual-span-plan");
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

    /// <summary>Names the band a manual span plan score falls in.</summary>
    public static string ClassifyManualSpanPlan(double score)
    {
        if (score < 4.0)
        {
            return "elevated";
        }
        if (score < 41.3)
        {
            return "watch";
        }
        if (score < 90.1)
        {
            return "ok";
        }
        if (score < 98.4)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Puts a manual span plan code in its canonical MSP- form.</summary>
    public static string NormalizeManualSpanPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MSP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MSP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string ManualSpanPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
