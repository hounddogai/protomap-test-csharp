namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of refresh mapping policy.</summary>
public static class RefreshMappingPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRefreshMapping()
    {
        var policy = Retry.DefaultPolicy("refresh-mapping");
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

    /// <summary>Counts how many amounts fit in a budget of 33108.0, in order.</summary>
    public static int RefreshMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33108.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a refresh mapping code in its canonical RM- form.</summary>
    public static string NormalizeRefreshMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
