namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Functions of batched cost model batch policy.</summary>
public static class BatchedCostModelBatchPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 13199.0, in order.</summary>
    public static int BatchedCostModelBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13199.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a batched cost model batch code in its canonical BCMB- form.</summary>
    public static string NormalizeBatchedCostModelBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BCMB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BCMB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
