namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionMilestoneChecks
{
    public static void ExceptionMilestoneStartsInNew()
    {
        var item = ExceptionMilestone.NewExceptionMilestone("code-607", "title-283");
        Check.Equal(item.Status, ExceptionMilestoneStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionMilestoneMovesToQueued()
    {
        var item = ExceptionMilestone.NewExceptionMilestone("code-607", "title-283");
        try
        {
            item.MoveTo(ExceptionMilestoneStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionMilestoneStatus.Queued, "status");
    }

    public static void ExceptionMilestoneRejectsMovesFromFailed()
    {
        var item = ExceptionMilestone.NewExceptionMilestone("code-607", "title-283");
        item.Status = ExceptionMilestoneStatus.Failed;
        Check.Fails(() => item.MoveTo(ExceptionMilestoneStatus.New), "move");
    }

    public static void RunAll()
    {
        ExceptionMilestoneStartsInNew();
        ExceptionMilestoneMovesToQueued();
        ExceptionMilestoneRejectsMovesFromFailed();
    }
}
