namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanRequestChecks
{
    public static void SuccessionPlanRequestStartsInNew()
    {
        var item = SuccessionPlanRequest.NewSuccessionPlanRequest("code-835", "label-658");
        Check.Equal(item.Status, SuccessionPlanRequestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanRequestMovesToDone()
    {
        var item = SuccessionPlanRequest.NewSuccessionPlanRequest("code-835", "label-658");
        try
        {
            item.MoveTo(SuccessionPlanRequestStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanRequestStatus.Done, "status");
    }

    public static void SuccessionPlanRequestRejectsMovesFromFailed()
    {
        var item = SuccessionPlanRequest.NewSuccessionPlanRequest("code-835", "label-658");
        item.Status = SuccessionPlanRequestStatus.Failed;
        Check.Fails(() => item.MoveTo(SuccessionPlanRequestStatus.New), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanRequestStartsInNew();
        SuccessionPlanRequestMovesToDone();
        SuccessionPlanRequestRejectsMovesFromFailed();
    }
}
