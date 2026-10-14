namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default reorg exception policy.</summary>
public static class DefaultReorgExceptionPolicy
{
    public static string DefaultReorgExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultReorgException()
    {
        var policy = Retry.DefaultPolicy("default-reorg-exception");
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

    /// <summary>Puts a default reorg exception code in its canonical DRE- form.</summary>
    public static string NormalizeDefaultReorgExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DRE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DRE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a default reorg exception score falls in.</summary>
    public static string ClassifyDefaultReorgException(double score)
    {
        if (score < 86.5)
        {
            return "watch";
        }
        if (score < 95.8)
        {
            return "medium";
        }
        return "ok";
    }
}
