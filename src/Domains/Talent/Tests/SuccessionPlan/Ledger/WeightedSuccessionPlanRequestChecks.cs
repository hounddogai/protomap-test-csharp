namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class WeightedSuccessionPlanRequestChecks
{
    public static void WeightedSuccessionPlanRequestStartsInNew()
    {
        var item = WeightedSuccessionPlanRequest.NewWeightedSuccessionPlanRequest("code-246", "label-390");
        Check.Equal(item.Status, WeightedSuccessionPlanRequestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedSuccessionPlanRequestMovesToQueued()
    {
        var item = WeightedSuccessionPlanRequest.NewWeightedSuccessionPlanRequest("code-246", "label-390");
        try
        {
            item.MoveTo(WeightedSuccessionPlanRequestStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedSuccessionPlanRequestStatus.Queued, "status");
    }

    public static void WeightedSuccessionPlanRequestRejectsMovesFromFailed()
    {
        var item = WeightedSuccessionPlanRequest.NewWeightedSuccessionPlanRequest("code-246", "label-390");
        item.Status = WeightedSuccessionPlanRequestStatus.Failed;
        Check.Fails(() => item.MoveTo(WeightedSuccessionPlanRequestStatus.New), "move");
    }

    public static void RunAll()
    {
        WeightedSuccessionPlanRequestStartsInNew();
        WeightedSuccessionPlanRequestMovesToQueued();
        WeightedSuccessionPlanRequestRejectsMovesFromFailed();
    }
}
