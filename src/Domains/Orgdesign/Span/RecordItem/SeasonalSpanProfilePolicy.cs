namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal span profile policy.</summary>
public static class SeasonalSpanProfilePolicy
{
    /// <summary>Puts a seasonal span profile code in its canonical SSP- form.</summary>
    public static string NormalizeSeasonalSpanProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SSP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SSP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 67747.0, in order.</summary>
    public static int SeasonalSpanProfileBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67747.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalSpanProfile()
    {
        var policy = Retry.DefaultPolicy("seasonal-span-profile");
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
