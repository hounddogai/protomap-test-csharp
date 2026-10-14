namespace Acme.Workforce.Payrollops.Tests.PayRun.Preference;

using Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledPayRunRequestChecks
{
    public static void ScheduledPayRunRequestStartsInNew()
    {
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("code-277", "name-155");
        Check.Equal(item.Status, ScheduledPayRunRequestStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPayRunRequestMovesToDone()
    {
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("code-277", "name-155");
        try
        {
            item.MoveTo(ScheduledPayRunRequestStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPayRunRequestStatus.Done, "status");
    }

    public static void ScheduledPayRunRequestRejectsMovesFromFailed()
    {
        var item = ScheduledPayRunRequest.NewScheduledPayRunRequest("code-277", "name-155");
        item.Status = ScheduledPayRunRequestStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledPayRunRequestStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledPayRunRequestStartsInNew();
        ScheduledPayRunRequestMovesToDone();
        ScheduledPayRunRequestRejectsMovesFromFailed();
    }
}
