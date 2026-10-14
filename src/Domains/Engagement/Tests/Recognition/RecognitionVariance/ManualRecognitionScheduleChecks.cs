namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class ManualRecognitionScheduleChecks
{
    public static void ManualRecognitionScheduleStartsInCreated()
    {
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("code-325", "title-418");
        Check.Equal(item.Status, ManualRecognitionScheduleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualRecognitionScheduleMovesToPublished()
    {
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("code-325", "title-418");
        try
        {
            item.MoveTo(ManualRecognitionScheduleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualRecognitionScheduleStatus.Published, "status");
    }

    public static void ManualRecognitionScheduleRejectsMovesFromRetired()
    {
        var item = ManualRecognitionSchedule.NewManualRecognitionSchedule("code-325", "title-418");
        item.Status = ManualRecognitionScheduleStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualRecognitionScheduleStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualRecognitionScheduleStartsInCreated();
        ManualRecognitionScheduleMovesToPublished();
        ManualRecognitionScheduleRejectsMovesFromRetired();
    }
}
