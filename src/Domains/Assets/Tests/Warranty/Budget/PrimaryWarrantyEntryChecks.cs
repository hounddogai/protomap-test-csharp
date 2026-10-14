namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PrimaryWarrantyEntryChecks
{
    public static void PrimaryWarrantyEntryStartsInNew()
    {
        var item = PrimaryWarrantyEntry.NewPrimaryWarrantyEntry("code-601", "title-474");
        Check.Equal(item.Status, PrimaryWarrantyEntryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryWarrantyEntryMovesToQueued()
    {
        var item = PrimaryWarrantyEntry.NewPrimaryWarrantyEntry("code-601", "title-474");
        try
        {
            item.MoveTo(PrimaryWarrantyEntryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryWarrantyEntryStatus.Queued, "status");
    }

    public static void PrimaryWarrantyEntryRejectsMovesFromFailed()
    {
        var item = PrimaryWarrantyEntry.NewPrimaryWarrantyEntry("code-601", "title-474");
        item.Status = PrimaryWarrantyEntryStatus.Failed;
        Check.Fails(() => item.MoveTo(PrimaryWarrantyEntryStatus.New), "move");
    }

    public static void RunAll()
    {
        PrimaryWarrantyEntryStartsInNew();
        PrimaryWarrantyEntryMovesToQueued();
        PrimaryWarrantyEntryRejectsMovesFromFailed();
    }
}
