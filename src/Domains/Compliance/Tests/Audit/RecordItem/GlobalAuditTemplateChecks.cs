namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalAuditTemplateChecks
{
    public static void GlobalAuditTemplateStartsInPlanned()
    {
        var item = GlobalAuditTemplate.NewGlobalAuditTemplate("code-513", "label-766");
        Check.Equal(item.Status, GlobalAuditTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAuditTemplateMovesToInProgress()
    {
        var item = GlobalAuditTemplate.NewGlobalAuditTemplate("code-513", "label-766");
        try
        {
            item.MoveTo(GlobalAuditTemplateStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAuditTemplateStatus.InProgress, "status");
    }

    public static void GlobalAuditTemplateRejectsMovesFromCancelled()
    {
        var item = GlobalAuditTemplate.NewGlobalAuditTemplate("code-513", "label-766");
        item.Status = GlobalAuditTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalAuditTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalAuditTemplateStartsInPlanned();
        GlobalAuditTemplateMovesToInProgress();
        GlobalAuditTemplateRejectsMovesFromCancelled();
    }
}
