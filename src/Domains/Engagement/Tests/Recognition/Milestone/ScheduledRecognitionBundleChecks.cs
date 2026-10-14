namespace Acme.Workforce.Engagement.Tests.Recognition.Milestone;

using Acme.Workforce.Engagement.Recognition.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ScheduledRecognitionBundleChecks
{
    public static void ScheduledRecognitionBundleStartsInOpen()
    {
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("code-319", "label-276");
        Check.Equal(item.Status, ScheduledRecognitionBundleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledRecognitionBundleMovesToResolved()
    {
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("code-319", "label-276");
        try
        {
            item.MoveTo(ScheduledRecognitionBundleStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledRecognitionBundleStatus.Resolved, "status");
    }

    public static void ScheduledRecognitionBundleRejectsMovesFromArchived()
    {
        var item = ScheduledRecognitionBundle.NewScheduledRecognitionBundle("code-319", "label-276");
        item.Status = ScheduledRecognitionBundleStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledRecognitionBundleStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledRecognitionBundleStartsInOpen();
        ScheduledRecognitionBundleMovesToResolved();
        ScheduledRecognitionBundleRejectsMovesFromArchived();
    }
}
