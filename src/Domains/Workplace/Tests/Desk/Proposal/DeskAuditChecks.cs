namespace Acme.Workforce.Workplace.Tests.Desk.Proposal;

using Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeskAuditChecks
{
    public static void DeskAuditStartsInNew()
    {
        var item = DeskAudit.NewDeskAudit("code-610", "label-231");
        Check.Equal(item.Status, DeskAuditStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskAuditMovesToQueued()
    {
        var item = DeskAudit.NewDeskAudit("code-610", "label-231");
        try
        {
            item.MoveTo(DeskAuditStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskAuditStatus.Queued, "status");
    }

    public static void DeskAuditRejectsMovesFromFailed()
    {
        var item = DeskAudit.NewDeskAudit("code-610", "label-231");
        item.Status = DeskAuditStatus.Failed;
        Check.Fails(() => item.MoveTo(DeskAuditStatus.New), "move");
    }

    public static void RunAll()
    {
        DeskAuditStartsInNew();
        DeskAuditMovesToQueued();
        DeskAuditRejectsMovesFromFailed();
    }
}
