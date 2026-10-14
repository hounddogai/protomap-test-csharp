namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict warranty manifest policy.</summary>
public static class StrictWarrantyManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 93526.0, in order.</summary>
    public static int StrictWarrantyManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93526.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictWarrantyManifest()
    {
        var policy = Retry.DefaultPolicy("strict-warranty-manifest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
