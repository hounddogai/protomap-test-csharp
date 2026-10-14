namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of headcount override policy.</summary>
public static class HeadcountOverridePolicy
{
    /// <summary>Puts a headcount override code in its canonical HO- form.</summary>
    public static string NormalizeHeadcountOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHeadcountOverride()
    {
        var policy = Retry.DefaultPolicy("headcount-override");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
