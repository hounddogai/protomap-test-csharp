namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PrimaryHeadcountAssignmentChecks
{
    public static void PrimaryHeadcountAssignmentStartsInProposed()
    {
        var item = PrimaryHeadcountAssignment.NewPrimaryHeadcountAssignment("code-728", "title-289");
        Check.Equal(item.Status, PrimaryHeadcountAssignmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryHeadcountAssignmentMovesToApproved()
    {
        var item = PrimaryHeadcountAssignment.NewPrimaryHeadcountAssignment("code-728", "title-289");
        try
        {
            item.MoveTo(PrimaryHeadcountAssignmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryHeadcountAssignmentStatus.Approved, "status");
    }

    public static void PrimaryHeadcountAssignmentRejectsMovesFromWithdrawn()
    {
        var item = PrimaryHeadcountAssignment.NewPrimaryHeadcountAssignment("code-728", "title-289");
        item.Status = PrimaryHeadcountAssignmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PrimaryHeadcountAssignmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PrimaryHeadcountAssignmentStartsInProposed();
        PrimaryHeadcountAssignmentMovesToApproved();
        PrimaryHeadcountAssignmentRejectsMovesFromWithdrawn();
    }
}
