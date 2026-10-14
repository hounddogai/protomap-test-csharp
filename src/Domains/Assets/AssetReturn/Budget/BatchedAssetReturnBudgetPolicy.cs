namespace Acme.Workforce.Assets.AssetReturn.Budget;

/// <summary>Functions of batched asset return budget policy.</summary>
public static class BatchedAssetReturnBudgetPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 99879.0, in order.</summary>
    public static int BatchedAssetReturnBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99879.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a batched asset return budget code in its canonical BARB- form.</summary>
    public static string NormalizeBatchedAssetReturnBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BARB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BARB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
