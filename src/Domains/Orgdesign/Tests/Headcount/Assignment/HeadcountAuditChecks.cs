namespace Acme.Workforce.Orgdesign.Tests.Headcount.Assignment;

using Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class HeadcountAuditChecks
{
    public static void HeadcountAuditStartsInOpen()
    {
        var item = HeadcountAudit.NewHeadcountAudit("code-764", "label-232");
        Check.Equal(item.Status, HeadcountAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountAuditMovesToInReview()
    {
        var item = HeadcountAudit.NewHeadcountAudit("code-764", "label-232");
        try
        {
            item.MoveTo(HeadcountAuditStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountAuditStatus.InReview, "status");
    }

    public static void HeadcountAuditRejectsMovesFromArchived()
    {
        var item = HeadcountAudit.NewHeadcountAudit("code-764", "label-232");
        item.Status = HeadcountAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(HeadcountAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        HeadcountAuditStartsInOpen();
        HeadcountAuditMovesToInReview();
        HeadcountAuditRejectsMovesFromArchived();
    }
}
