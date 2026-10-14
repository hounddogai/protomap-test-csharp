namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyEntryChecks
{
    public static void WarrantyEntryStartsInPlanned()
    {
        var item = WarrantyEntry.NewWarrantyEntry("code-609", "label-907");
        Check.Equal(item.Status, WarrantyEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyEntryMovesToCancelled()
    {
        var item = WarrantyEntry.NewWarrantyEntry("code-609", "label-907");
        try
        {
            item.MoveTo(WarrantyEntryStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyEntryStatus.Cancelled, "status");
    }

    public static void WarrantyEntryRejectsMovesFromCancelled()
    {
        var item = WarrantyEntry.NewWarrantyEntry("code-609", "label-907");
        item.Status = WarrantyEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WarrantyEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WarrantyEntryStartsInPlanned();
        WarrantyEntryMovesToCancelled();
        WarrantyEntryRejectsMovesFromCancelled();
    }
}
