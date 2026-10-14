namespace Acme.Workforce.Compliance.Tests.Audit.Tier;

using Acme.Workforce.Compliance.Audit.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class SeasonalAuditAssignmentChecks
{
    public static void SeasonalAuditAssignmentStartsInDraft()
    {
        var item = SeasonalAuditAssignment.NewSeasonalAuditAssignment("code-925", "name-265");
        Check.Equal(item.Status, SeasonalAuditAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAuditAssignmentMovesToPending()
    {
        var item = SeasonalAuditAssignment.NewSeasonalAuditAssignment("code-925", "name-265");
        try
        {
            item.MoveTo(SeasonalAuditAssignmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAuditAssignmentStatus.Pending, "status");
    }

    public static void SeasonalAuditAssignmentRejectsMovesFromClosed()
    {
        var item = SeasonalAuditAssignment.NewSeasonalAuditAssignment("code-925", "name-265");
        item.Status = SeasonalAuditAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalAuditAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalAuditAssignmentStartsInDraft();
        SeasonalAuditAssignmentMovesToPending();
        SeasonalAuditAssignmentRejectsMovesFromClosed();
    }
}
