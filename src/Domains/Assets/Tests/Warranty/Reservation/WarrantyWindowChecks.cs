namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyWindowChecks
{
    public static void WarrantyWindowStartsInDraft()
    {
        var item = WarrantyWindow.NewWarrantyWindow("code-193", "label-795");
        Check.Equal(item.Status, WarrantyWindowStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyWindowMovesToPending()
    {
        var item = WarrantyWindow.NewWarrantyWindow("code-193", "label-795");
        try
        {
            item.MoveTo(WarrantyWindowStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyWindowStatus.Pending, "status");
    }

    public static void WarrantyWindowRejectsMovesFromClosed()
    {
        var item = WarrantyWindow.NewWarrantyWindow("code-193", "label-795");
        item.Status = WarrantyWindowStatus.Closed;
        Check.Fails(() => item.MoveTo(WarrantyWindowStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WarrantyWindowStartsInDraft();
        WarrantyWindowMovesToPending();
        WarrantyWindowRejectsMovesFromClosed();
    }
}
