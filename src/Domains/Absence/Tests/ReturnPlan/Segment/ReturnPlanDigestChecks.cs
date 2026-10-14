namespace Acme.Workforce.Absence.Tests.ReturnPlan.Segment;

using Acme.Workforce.Absence.ReturnPlan.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ReturnPlanDigestChecks
{
    public static void ReturnPlanDigestStartsInIdle()
    {
        var item = ReturnPlanDigest.NewReturnPlanDigest("code-614", "name-754");
        Check.Equal(item.Status, ReturnPlanDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanDigestMovesToReady()
    {
        var item = ReturnPlanDigest.NewReturnPlanDigest("code-614", "name-754");
        try
        {
            item.MoveTo(ReturnPlanDigestStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanDigestStatus.Ready, "status");
    }

    public static void ReturnPlanDigestRejectsMovesFromStopped()
    {
        var item = ReturnPlanDigest.NewReturnPlanDigest("code-614", "name-754");
        item.Status = ReturnPlanDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReturnPlanDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReturnPlanDigestStartsInIdle();
        ReturnPlanDigestMovesToReady();
        ReturnPlanDigestRejectsMovesFromStopped();
    }
}
