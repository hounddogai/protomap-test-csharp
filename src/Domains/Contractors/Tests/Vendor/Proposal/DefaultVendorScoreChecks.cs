namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultVendorScoreChecks
{
    public static void DefaultVendorScoreStartsInDraft()
    {
        var item = DefaultVendorScore.NewDefaultVendorScore("code-630", "name-593");
        Check.Equal(item.Status, DefaultVendorScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultVendorScoreMovesToSuspended()
    {
        var item = DefaultVendorScore.NewDefaultVendorScore("code-630", "name-593");
        try
        {
            item.MoveTo(DefaultVendorScoreStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultVendorScoreStatus.Suspended, "status");
    }

    public static void DefaultVendorScoreRejectsMovesFromClosed()
    {
        var item = DefaultVendorScore.NewDefaultVendorScore("code-630", "name-593");
        item.Status = DefaultVendorScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(DefaultVendorScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DefaultVendorScoreStartsInDraft();
        DefaultVendorScoreMovesToSuspended();
        DefaultVendorScoreRejectsMovesFromClosed();
    }
}
