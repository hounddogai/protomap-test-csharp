namespace Acme.Workforce.Contractors.Tests.Vendor.Score;

using Acme.Workforce.Contractors.Vendor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class VendorScoreChecks
{
    public static void VendorScoreStartsInDraft()
    {
        var item = VendorScore.NewVendorScore("code-745", "label-433");
        Check.Equal(item.Status, VendorScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorScoreMovesToSuspended()
    {
        var item = VendorScore.NewVendorScore("code-745", "label-433");
        try
        {
            item.MoveTo(VendorScoreStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorScoreStatus.Suspended, "status");
    }

    public static void VendorScoreRejectsMovesFromClosed()
    {
        var item = VendorScore.NewVendorScore("code-745", "label-433");
        item.Status = VendorScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(VendorScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        VendorScoreStartsInDraft();
        VendorScoreMovesToSuspended();
        VendorScoreRejectsMovesFromClosed();
    }
}
