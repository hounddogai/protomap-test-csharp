namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyBaselineChecks
{
    public static void WarrantyBaselineStartsInNew()
    {
        var item = WarrantyBaseline.NewWarrantyBaseline("code-278", "label-977");
        Check.Equal(item.Status, WarrantyBaselineStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyBaselineMovesToQueued()
    {
        var item = WarrantyBaseline.NewWarrantyBaseline("code-278", "label-977");
        try
        {
            item.MoveTo(WarrantyBaselineStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyBaselineStatus.Queued, "status");
    }

    public static void WarrantyBaselineRejectsMovesFromFailed()
    {
        var item = WarrantyBaseline.NewWarrantyBaseline("code-278", "label-977");
        item.Status = WarrantyBaselineStatus.Failed;
        Check.Fails(() => item.MoveTo(WarrantyBaselineStatus.New), "move");
    }

    public static void RunAll()
    {
        WarrantyBaselineStartsInNew();
        WarrantyBaselineMovesToQueued();
        WarrantyBaselineRejectsMovesFromFailed();
    }
}
