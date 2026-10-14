namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RecognitionScheduleChecks
{
    public static void RecognitionScheduleStartsInOpen()
    {
        var item = RecognitionSchedule.NewRecognitionSchedule("code-878", "title-180");
        Check.Equal(item.Status, RecognitionScheduleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionScheduleMovesToInReview()
    {
        var item = RecognitionSchedule.NewRecognitionSchedule("code-878", "title-180");
        try
        {
            item.MoveTo(RecognitionScheduleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionScheduleStatus.InReview, "status");
    }

    public static void RecognitionScheduleRejectsMovesFromArchived()
    {
        var item = RecognitionSchedule.NewRecognitionSchedule("code-878", "title-180");
        item.Status = RecognitionScheduleStatus.Archived;
        Check.Fails(() => item.MoveTo(RecognitionScheduleStatus.Open), "move");
    }

    public static void RunAll()
    {
        RecognitionScheduleStartsInOpen();
        RecognitionScheduleMovesToInReview();
        RecognitionScheduleRejectsMovesFromArchived();
    }
}
