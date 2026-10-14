namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyGroupChecks
{
    public static void WarrantyGroupStartsInDraft()
    {
        var item = WarrantyGroup.NewWarrantyGroup("code-824", "name-685");
        Check.Equal(item.Status, WarrantyGroupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyGroupMovesToSuspended()
    {
        var item = WarrantyGroup.NewWarrantyGroup("code-824", "name-685");
        try
        {
            item.MoveTo(WarrantyGroupStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyGroupStatus.Suspended, "status");
    }

    public static void WarrantyGroupRejectsMovesFromClosed()
    {
        var item = WarrantyGroup.NewWarrantyGroup("code-824", "name-685");
        item.Status = WarrantyGroupStatus.Closed;
        Check.Fails(() => item.MoveTo(WarrantyGroupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WarrantyGroupStartsInDraft();
        WarrantyGroupMovesToSuspended();
        WarrantyGroupRejectsMovesFromClosed();
    }
}
