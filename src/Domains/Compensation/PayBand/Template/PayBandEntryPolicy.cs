namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay band entry policy.</summary>
public static class PayBandEntryPolicy
{
    /// <summary>Puts a pay band entry code in its canonical PBE- form.</summary>
    public static string NormalizePayBandEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 26478.0, in order.</summary>
    public static int PayBandEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26478.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayBandEntry()
    {
        var policy = Retry.DefaultPolicy("pay-band-entry");
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

    /// <summary>Names the band a pay band entry score falls in.</summary>
    public static string ClassifyPayBandEntry(double score)
    {
        if (score < 79.0)
        {
            return "elevated";
        }
        if (score < 93.4)
        {
            return "watch";
        }
        return "high";
    }
}
