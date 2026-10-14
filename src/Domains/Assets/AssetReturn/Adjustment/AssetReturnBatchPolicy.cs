namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return batch policy.</summary>
public static class AssetReturnBatchPolicy
{
    public static int AssetReturnBatchSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnBatch()
    {
        var policy = Retry.DefaultPolicy("asset-return-batch");
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

    /// <summary>Counts how many amounts fit in a budget of 14636.0, in order.</summary>
    public static int AssetReturnBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14636.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a asset return batch code in its canonical ARB- form.</summary>
    public static string NormalizeAssetReturnBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
