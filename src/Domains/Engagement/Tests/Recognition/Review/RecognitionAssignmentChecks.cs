namespace Acme.Workforce.Engagement.Tests.Recognition.Review;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RecognitionAssignmentChecks
{
    public static void RecognitionAssignmentStartsInDraft()
    {
        var item = RecognitionAssignment.NewRecognitionAssignment("code-104", "title-499");
        Check.Equal(item.Status, RecognitionAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionAssignmentMovesToActive()
    {
        var item = RecognitionAssignment.NewRecognitionAssignment("code-104", "title-499");
        try
        {
            item.MoveTo(RecognitionAssignmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionAssignmentStatus.Active, "status");
    }

    public static void RecognitionAssignmentRejectsMovesFromClosed()
    {
        var item = RecognitionAssignment.NewRecognitionAssignment("code-104", "title-499");
        item.Status = RecognitionAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(RecognitionAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RecognitionAssignmentStartsInDraft();
        RecognitionAssignmentMovesToActive();
        RecognitionAssignmentRejectsMovesFromClosed();
    }
}
