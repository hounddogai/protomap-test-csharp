namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentAuditChecks
{
    public static void AssignmentAuditStartsInOpen()
    {
        var item = AssignmentAudit.NewAssignmentAudit("code-696", "label-17");
        Check.Equal(item.Status, AssignmentAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentAuditMovesToResolved()
    {
        var item = AssignmentAudit.NewAssignmentAudit("code-696", "label-17");
        try
        {
            item.MoveTo(AssignmentAuditStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentAuditStatus.Resolved, "status");
    }

    public static void AssignmentAuditRejectsMovesFromArchived()
    {
        var item = AssignmentAudit.NewAssignmentAudit("code-696", "label-17");
        item.Status = AssignmentAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(AssignmentAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssignmentAuditStartsInOpen();
        AssignmentAuditMovesToResolved();
        AssignmentAuditRejectsMovesFromArchived();
    }
}
