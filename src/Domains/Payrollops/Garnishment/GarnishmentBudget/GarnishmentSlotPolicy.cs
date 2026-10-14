namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of garnishment slot policy.</summary>
public static class GarnishmentSlotPolicy
{
    /// <summary>Names the band a garnishment slot score falls in.</summary>
    public static string ClassifyGarnishmentSlot(double score)
    {
        if (score < 20.0)
        {
            return "low";
        }
        if (score < 63.9)
        {
            return "critical";
        }
        return "high";
    }

    /// <summary>Puts a garnishment slot code in its canonical GS- form.</summary>
    public static string NormalizeGarnishmentSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string GarnishmentSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGarnishmentSlot()
    {
        var policy = Retry.DefaultPolicy("garnishment-slot");
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
