namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class ManualWarrantyReservationChecks
{
    public static void ManualWarrantyReservationStartsInCreated()
    {
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("code-276", "label-834");
        Check.Equal(item.Status, ManualWarrantyReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualWarrantyReservationMovesToValidated()
    {
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("code-276", "label-834");
        try
        {
            item.MoveTo(ManualWarrantyReservationStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualWarrantyReservationStatus.Validated, "status");
    }

    public static void ManualWarrantyReservationRejectsMovesFromRetired()
    {
        var item = ManualWarrantyReservation.NewManualWarrantyReservation("code-276", "label-834");
        item.Status = ManualWarrantyReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualWarrantyReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualWarrantyReservationStartsInCreated();
        ManualWarrantyReservationMovesToValidated();
        ManualWarrantyReservationRejectsMovesFromRetired();
    }
}
