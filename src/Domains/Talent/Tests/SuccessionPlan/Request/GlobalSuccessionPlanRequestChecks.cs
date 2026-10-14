namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Request;

using Acme.Workforce.Talent.SuccessionPlan.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class GlobalSuccessionPlanRequestChecks
{
    public static void GlobalSuccessionPlanRequestStartsInDraft()
    {
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("code-808", "label-269");
        Check.Equal(item.Status, GlobalSuccessionPlanRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalSuccessionPlanRequestMovesToClosed()
    {
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("code-808", "label-269");
        try
        {
            item.MoveTo(GlobalSuccessionPlanRequestStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalSuccessionPlanRequestStatus.Closed, "status");
    }

    public static void GlobalSuccessionPlanRequestRejectsMovesFromClosed()
    {
        var item = GlobalSuccessionPlanRequest.NewGlobalSuccessionPlanRequest("code-808", "label-269");
        item.Status = GlobalSuccessionPlanRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalSuccessionPlanRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalSuccessionPlanRequestStartsInDraft();
        GlobalSuccessionPlanRequestMovesToClosed();
        GlobalSuccessionPlanRequestRejectsMovesFromClosed();
    }
}
