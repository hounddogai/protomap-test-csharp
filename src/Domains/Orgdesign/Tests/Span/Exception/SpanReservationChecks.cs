namespace Acme.Workforce.Orgdesign.Tests.Span.Exception;

using Acme.Workforce.Orgdesign.Span.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class SpanReservationChecks
{
    public static void SpanReservationStartsInRequested()
    {
        var item = SpanReservation.NewSpanReservation("code-219", "title-253");
        Check.Equal(item.Status, SpanReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanReservationMovesToGranted()
    {
        var item = SpanReservation.NewSpanReservation("code-219", "title-253");
        try
        {
            item.MoveTo(SpanReservationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanReservationStatus.Granted, "status");
    }

    public static void SpanReservationRejectsMovesFromExpired()
    {
        var item = SpanReservation.NewSpanReservation("code-219", "title-253");
        item.Status = SpanReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(SpanReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SpanReservationStartsInRequested();
        SpanReservationMovesToGranted();
        SpanReservationRejectsMovesFromExpired();
    }
}
