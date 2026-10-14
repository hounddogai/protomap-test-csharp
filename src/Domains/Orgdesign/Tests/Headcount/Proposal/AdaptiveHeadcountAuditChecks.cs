namespace Acme.Workforce.Orgdesign.Tests.Headcount.Proposal;

using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AdaptiveHeadcountAuditChecks
{
    public static void AdaptiveHeadcountAuditStartsInOpen()
    {
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("code-192", "label-847");
        Check.Equal(item.Status, AdaptiveHeadcountAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveHeadcountAuditMovesToReopened()
    {
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("code-192", "label-847");
        try
        {
            item.MoveTo(AdaptiveHeadcountAuditStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveHeadcountAuditStatus.Reopened, "status");
    }

    public static void AdaptiveHeadcountAuditRejectsMovesFromArchived()
    {
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("code-192", "label-847");
        item.Status = AdaptiveHeadcountAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveHeadcountAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountAuditStartsInOpen();
        AdaptiveHeadcountAuditMovesToReopened();
        AdaptiveHeadcountAuditRejectsMovesFromArchived();
    }
}
