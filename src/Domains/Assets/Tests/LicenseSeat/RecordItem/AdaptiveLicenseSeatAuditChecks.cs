namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatAuditChecks
{
    public static void AdaptiveLicenseSeatAuditStartsInOpen()
    {
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("code-627", "label-563");
        Check.Equal(item.Status, AdaptiveLicenseSeatAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveLicenseSeatAuditMovesToInReview()
    {
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("code-627", "label-563");
        try
        {
            item.MoveTo(AdaptiveLicenseSeatAuditStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveLicenseSeatAuditStatus.InReview, "status");
    }

    public static void AdaptiveLicenseSeatAuditRejectsMovesFromArchived()
    {
        var item = AdaptiveLicenseSeatAudit.NewAdaptiveLicenseSeatAudit("code-627", "label-563");
        item.Status = AdaptiveLicenseSeatAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveLicenseSeatAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveLicenseSeatAuditStartsInOpen();
        AdaptiveLicenseSeatAuditMovesToInReview();
        AdaptiveLicenseSeatAuditRejectsMovesFromArchived();
    }
}
