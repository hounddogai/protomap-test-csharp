namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedDeductionPlanChecks
{
    public static void WeightedDeductionPlanStartsInOpen()
    {
        var item = WeightedDeductionPlan.NewWeightedDeductionPlan("code-461", "label-666");
        Check.Equal(item.Status, WeightedDeductionPlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedDeductionPlanMovesToReopened()
    {
        var item = WeightedDeductionPlan.NewWeightedDeductionPlan("code-461", "label-666");
        try
        {
            item.MoveTo(WeightedDeductionPlanStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedDeductionPlanStatus.Reopened, "status");
    }

    public static void WeightedDeductionPlanRejectsMovesFromArchived()
    {
        var item = WeightedDeductionPlan.NewWeightedDeductionPlan("code-461", "label-666");
        item.Status = WeightedDeductionPlanStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedDeductionPlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedDeductionPlanStartsInOpen();
        WeightedDeductionPlanMovesToReopened();
        WeightedDeductionPlanRejectsMovesFromArchived();
    }
}
