namespace Acme.Workforce.Assets.AssetReturn.Segment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of asset return target policy.</summary>
public static class AssetReturnTargetPolicy
{
    /// <summary>Puts a asset return target code in its canonical ART- form.</summary>
    public static string NormalizeAssetReturnTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ART-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ART-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssetReturnTarget()
    {
        var policy = Retry.DefaultPolicy("asset-return-target");
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

    /// <summary>Counts how many amounts fit in a budget of 59835.0, in order.</summary>
    public static int AssetReturnTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 59835.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
