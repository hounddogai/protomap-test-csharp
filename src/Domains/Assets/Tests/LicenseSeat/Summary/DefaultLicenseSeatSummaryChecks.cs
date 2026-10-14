namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultLicenseSeatSummaryChecks
{
    public static void DefaultLicenseSeatSummaryStartsInProposed()
    {
        var item = DefaultLicenseSeatSummary.NewDefaultLicenseSeatSummary("code-50", "title-305");
        Check.Equal(item.Status, DefaultLicenseSeatSummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultLicenseSeatSummaryMovesToApproved()
    {
        var item = DefaultLicenseSeatSummary.NewDefaultLicenseSeatSummary("code-50", "title-305");
        try
        {
            item.MoveTo(DefaultLicenseSeatSummaryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultLicenseSeatSummaryStatus.Approved, "status");
    }

    public static void DefaultLicenseSeatSummaryRejectsMovesFromWithdrawn()
    {
        var item = DefaultLicenseSeatSummary.NewDefaultLicenseSeatSummary("code-50", "title-305");
        item.Status = DefaultLicenseSeatSummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultLicenseSeatSummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultLicenseSeatSummaryStartsInProposed();
        DefaultLicenseSeatSummaryMovesToApproved();
        DefaultLicenseSeatSummaryRejectsMovesFromWithdrawn();
    }
}
