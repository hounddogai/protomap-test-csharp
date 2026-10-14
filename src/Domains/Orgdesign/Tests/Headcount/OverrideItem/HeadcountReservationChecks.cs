namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountReservationChecks
{
    public static void HeadcountReservationStartsInProposed()
    {
        var item = HeadcountReservation.NewHeadcountReservation("code-390", "title-713");
        Check.Equal(item.Status, HeadcountReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountReservationMovesToApproved()
    {
        var item = HeadcountReservation.NewHeadcountReservation("code-390", "title-713");
        try
        {
            item.MoveTo(HeadcountReservationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountReservationStatus.Approved, "status");
    }

    public static void HeadcountReservationRejectsMovesFromWithdrawn()
    {
        var item = HeadcountReservation.NewHeadcountReservation("code-390", "title-713");
        item.Status = HeadcountReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(HeadcountReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        HeadcountReservationStartsInProposed();
        HeadcountReservationMovesToApproved();
        HeadcountReservationRejectsMovesFromWithdrawn();
    }
}
