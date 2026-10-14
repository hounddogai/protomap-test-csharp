namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class EngagementAuditChecks
{
    public static void EngagementAuditStartsInOpen()
    {
        var item = EngagementAudit.NewEngagementAudit("code-396", "name-419");
        Check.Equal(item.Status, EngagementAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementAuditMovesToResolved()
    {
        var item = EngagementAudit.NewEngagementAudit("code-396", "name-419");
        try
        {
            item.MoveTo(EngagementAuditStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementAuditStatus.Resolved, "status");
    }

    public static void EngagementAuditRejectsMovesFromArchived()
    {
        var item = EngagementAudit.NewEngagementAudit("code-396", "name-419");
        item.Status = EngagementAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(EngagementAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        EngagementAuditStartsInOpen();
        EngagementAuditMovesToResolved();
        EngagementAuditRejectsMovesFromArchived();
    }
}
