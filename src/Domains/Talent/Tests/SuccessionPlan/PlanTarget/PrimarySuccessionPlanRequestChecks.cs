namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class PrimarySuccessionPlanRequestChecks
{
    public static void PrimarySuccessionPlanRequestStartsInDraft()
    {
        var item = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest("code-458", "title-701");
        Check.Equal(item.Status, PrimarySuccessionPlanRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimarySuccessionPlanRequestMovesToPending()
    {
        var item = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest("code-458", "title-701");
        try
        {
            item.MoveTo(PrimarySuccessionPlanRequestStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimarySuccessionPlanRequestStatus.Pending, "status");
    }

    public static void PrimarySuccessionPlanRequestRejectsMovesFromClosed()
    {
        var item = PrimarySuccessionPlanRequest.NewPrimarySuccessionPlanRequest("code-458", "title-701");
        item.Status = PrimarySuccessionPlanRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimarySuccessionPlanRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimarySuccessionPlanRequestStartsInDraft();
        PrimarySuccessionPlanRequestMovesToPending();
        PrimarySuccessionPlanRequestRejectsMovesFromClosed();
    }
}
