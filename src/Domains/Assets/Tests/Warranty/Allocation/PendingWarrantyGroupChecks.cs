namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PendingWarrantyGroupChecks
{
    public static void PendingWarrantyGroupStartsInNew()
    {
        var item = PendingWarrantyGroup.NewPendingWarrantyGroup("code-413", "title-622");
        Check.Equal(item.Status, PendingWarrantyGroupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingWarrantyGroupMovesToDone()
    {
        var item = PendingWarrantyGroup.NewPendingWarrantyGroup("code-413", "title-622");
        try
        {
            item.MoveTo(PendingWarrantyGroupStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingWarrantyGroupStatus.Done, "status");
    }

    public static void PendingWarrantyGroupRejectsMovesFromFailed()
    {
        var item = PendingWarrantyGroup.NewPendingWarrantyGroup("code-413", "title-622");
        item.Status = PendingWarrantyGroupStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingWarrantyGroupStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingWarrantyGroupStartsInNew();
        PendingWarrantyGroupMovesToDone();
        PendingWarrantyGroupRejectsMovesFromFailed();
    }
}
