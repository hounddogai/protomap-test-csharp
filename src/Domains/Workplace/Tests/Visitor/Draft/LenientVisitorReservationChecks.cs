namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientVisitorReservationChecks
{
    public static void LenientVisitorReservationStartsInOpen()
    {
        var item = LenientVisitorReservation.NewLenientVisitorReservation("code-937", "name-953");
        Check.Equal(item.Status, LenientVisitorReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientVisitorReservationMovesToInReview()
    {
        var item = LenientVisitorReservation.NewLenientVisitorReservation("code-937", "name-953");
        try
        {
            item.MoveTo(LenientVisitorReservationStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientVisitorReservationStatus.InReview, "status");
    }

    public static void LenientVisitorReservationRejectsMovesFromArchived()
    {
        var item = LenientVisitorReservation.NewLenientVisitorReservation("code-937", "name-953");
        item.Status = LenientVisitorReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientVisitorReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientVisitorReservationStartsInOpen();
        LenientVisitorReservationMovesToInReview();
        LenientVisitorReservationRejectsMovesFromArchived();
    }
}
