namespace Acme.Workforce.Orgdesign.Tests.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

/// <summary>Checks of adaptive model policy code, run by the project's runner.</summary>
public static class WeightedCostModelPolicyChecks
{
    public static void WeightedCostModelPolicyStartsInNew()
    {
        var item = WeightedCostModelPolicy.NewWeightedCostModelPolicy("code-568", "title-578");
        Check.Equal(item.Status, WeightedCostModelPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedCostModelPolicyMovesToQueued()
    {
        var item = WeightedCostModelPolicy.NewWeightedCostModelPolicy("code-568", "title-578");
        try
        {
            item.MoveTo(WeightedCostModelPolicyStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedCostModelPolicyStatus.Queued, "status");
    }

    public static void WeightedCostModelPolicyRejectsMovesFromFailed()
    {
        var item = WeightedCostModelPolicy.NewWeightedCostModelPolicy("code-568", "title-578");
        item.Status = WeightedCostModelPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(WeightedCostModelPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        WeightedCostModelPolicyStartsInNew();
        WeightedCostModelPolicyMovesToQueued();
        WeightedCostModelPolicyRejectsMovesFromFailed();
    }
}
