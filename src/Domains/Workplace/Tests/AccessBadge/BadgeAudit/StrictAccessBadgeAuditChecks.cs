namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Checks of badge audit code, run by the project's runner.</summary>
public static class StrictAccessBadgeAuditChecks
{
    public static void StrictAccessBadgeAuditStartsInDraft()
    {
        var item = StrictAccessBadgeAudit.NewStrictAccessBadgeAudit("code-305", "label-783");
        Check.Equal(item.Status, StrictAccessBadgeAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAccessBadgeAuditMovesToPending()
    {
        var item = StrictAccessBadgeAudit.NewStrictAccessBadgeAudit("code-305", "label-783");
        try
        {
            item.MoveTo(StrictAccessBadgeAuditStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAccessBadgeAuditStatus.Pending, "status");
    }

    public static void StrictAccessBadgeAuditRejectsMovesFromClosed()
    {
        var item = StrictAccessBadgeAudit.NewStrictAccessBadgeAudit("code-305", "label-783");
        item.Status = StrictAccessBadgeAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictAccessBadgeAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictAccessBadgeAuditStartsInDraft();
        StrictAccessBadgeAuditMovesToPending();
        StrictAccessBadgeAuditRejectsMovesFromClosed();
    }
}
