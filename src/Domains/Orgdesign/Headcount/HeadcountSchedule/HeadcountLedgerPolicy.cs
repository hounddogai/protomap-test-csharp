namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of headcount ledger policy.</summary>
public static class HeadcountLedgerPolicy
{
    public static double HeadcountLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a headcount ledger score falls in.</summary>
    public static string ClassifyHeadcountLedger(double score)
    {
        if (score < 29.4)
        {
            return "low";
        }
        if (score < 86.7)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Puts a headcount ledger code in its canonical HL- form.</summary>
    public static string NormalizeHeadcountLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHeadcountLedger()
    {
        var policy = Retry.DefaultPolicy("headcount-ledger");
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
