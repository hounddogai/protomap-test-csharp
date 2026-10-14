namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of cost model record policy.</summary>
public static class CostModelRecordPolicy
{
    /// <summary>Puts a cost model record code in its canonical CMR- form.</summary>
    public static string NormalizeCostModelRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 23056.0, in order.</summary>
    public static int CostModelRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 23056.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCostModelRecord()
    {
        var policy = Retry.DefaultPolicy("cost-model-record");
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
