namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RollingRecognitionReservationChecks
{
    public static void RollingRecognitionReservationStartsInOpen()
    {
        var item = RollingRecognitionReservation.NewRollingRecognitionReservation("code-385", "title-361");
        Check.Equal(item.Status, RollingRecognitionReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingRecognitionReservationMovesToInReview()
    {
        var item = RollingRecognitionReservation.NewRollingRecognitionReservation("code-385", "title-361");
        try
        {
            item.MoveTo(RollingRecognitionReservationStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingRecognitionReservationStatus.InReview, "status");
    }

    public static void RollingRecognitionReservationRejectsMovesFromArchived()
    {
        var item = RollingRecognitionReservation.NewRollingRecognitionReservation("code-385", "title-361");
        item.Status = RollingRecognitionReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingRecognitionReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingRecognitionReservationStartsInOpen();
        RollingRecognitionReservationMovesToInReview();
        RollingRecognitionReservationRejectsMovesFromArchived();
    }
}
