namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return allocation policy.</summary>
public static class AssetReturnAllocationPolicy
{
    /// <summary>Names the band a asset return allocation score falls in.</summary>
    public static string ClassifyAssetReturnAllocation(double score)
    {
        if (score < 61.1)
        {
            return "ok";
        }
        if (score < 65.1)
        {
            return "high";
        }
        if (score < 86.0)
        {
            return "critical";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnAllocation()
    {
        var policy = Retry.DefaultPolicy("asset-return-allocation");
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

    /// <summary>Counts how many amounts fit in a budget of 64245.0, in order.</summary>
    public static int AssetReturnAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64245.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
