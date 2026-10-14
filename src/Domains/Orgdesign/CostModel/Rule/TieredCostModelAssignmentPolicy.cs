namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered cost model assignment policy.</summary>
public static class TieredCostModelAssignmentPolicy
{
    /// <summary>Puts a tiered cost model assignment code in its canonical TCMA- form.</summary>
    public static string NormalizeTieredCostModelAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TCMA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TCMA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 38708.0, in order.</summary>
    public static int TieredCostModelAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38708.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredCostModelAssignment()
    {
        var policy = Retry.DefaultPolicy("tiered-cost-model-assignment");
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
