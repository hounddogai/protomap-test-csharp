namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingRecognitionSlotChecks
{
    public static void RollingRecognitionSlotStartsInOpen()
    {
        var item = RollingRecognitionSlot.NewRollingRecognitionSlot("code-886", "label-843");
        Check.Equal(item.Status, RollingRecognitionSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingRecognitionSlotMovesToInReview()
    {
        var item = RollingRecognitionSlot.NewRollingRecognitionSlot("code-886", "label-843");
        try
        {
            item.MoveTo(RollingRecognitionSlotStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingRecognitionSlotStatus.InReview, "status");
    }

    public static void RollingRecognitionSlotRejectsMovesFromArchived()
    {
        var item = RollingRecognitionSlot.NewRollingRecognitionSlot("code-886", "label-843");
        item.Status = RollingRecognitionSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingRecognitionSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingRecognitionSlotStartsInOpen();
        RollingRecognitionSlotMovesToInReview();
        RollingRecognitionSlotRejectsMovesFromArchived();
    }
}
