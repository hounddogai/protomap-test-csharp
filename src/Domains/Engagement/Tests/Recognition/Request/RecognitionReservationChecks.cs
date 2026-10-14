namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionReservationChecks
{
    public static void RecognitionReservationStartsInIdle()
    {
        var item = RecognitionReservation.NewRecognitionReservation("code-804", "title-59");
        Check.Equal(item.Status, RecognitionReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionReservationMovesToDraining()
    {
        var item = RecognitionReservation.NewRecognitionReservation("code-804", "title-59");
        try
        {
            item.MoveTo(RecognitionReservationStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionReservationStatus.Draining, "status");
    }

    public static void RecognitionReservationRejectsMovesFromStopped()
    {
        var item = RecognitionReservation.NewRecognitionReservation("code-804", "title-59");
        item.Status = RecognitionReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(RecognitionReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RecognitionReservationStartsInIdle();
        RecognitionReservationMovesToDraining();
        RecognitionReservationRejectsMovesFromStopped();
    }
}
