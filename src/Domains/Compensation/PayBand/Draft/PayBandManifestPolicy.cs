namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay band manifest policy.</summary>
public static class PayBandManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 7417.0, in order.</summary>
    public static int PayBandManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7417.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayBandManifest()
    {
        var policy = Retry.DefaultPolicy("pay-band-manifest");
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
