namespace Acme.Workforce.Contractors.Tests.Engagement.Ledger;

using Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CappedEngagementEntryChecks
{
    public static void CappedEngagementEntryStartsInOpen()
    {
        var item = CappedEngagementEntry.NewCappedEngagementEntry("code-528", "label-180");
        Check.Equal(item.Status, CappedEngagementEntryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedEngagementEntryMovesToInReview()
    {
        var item = CappedEngagementEntry.NewCappedEngagementEntry("code-528", "label-180");
        try
        {
            item.MoveTo(CappedEngagementEntryStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedEngagementEntryStatus.InReview, "status");
    }

    public static void CappedEngagementEntryRejectsMovesFromArchived()
    {
        var item = CappedEngagementEntry.NewCappedEngagementEntry("code-528", "label-180");
        item.Status = CappedEngagementEntryStatus.Archived;
        Check.Fails(() => item.MoveTo(CappedEngagementEntryStatus.Open), "move");
    }

    public static void RunAll()
    {
        CappedEngagementEntryStartsInOpen();
        CappedEngagementEntryMovesToInReview();
        CappedEngagementEntryRejectsMovesFromArchived();
    }
}
