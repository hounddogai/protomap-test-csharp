namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class PrimaryVisitorAssignmentChecks
{
    public static void PrimaryVisitorAssignmentStartsInOpen()
    {
        var item = PrimaryVisitorAssignment.NewPrimaryVisitorAssignment("code-991", "label-381");
        Check.Equal(item.Status, PrimaryVisitorAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryVisitorAssignmentMovesToInReview()
    {
        var item = PrimaryVisitorAssignment.NewPrimaryVisitorAssignment("code-991", "label-381");
        try
        {
            item.MoveTo(PrimaryVisitorAssignmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryVisitorAssignmentStatus.InReview, "status");
    }

    public static void PrimaryVisitorAssignmentRejectsMovesFromArchived()
    {
        var item = PrimaryVisitorAssignment.NewPrimaryVisitorAssignment("code-991", "label-381");
        item.Status = PrimaryVisitorAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(PrimaryVisitorAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        PrimaryVisitorAssignmentStartsInOpen();
        PrimaryVisitorAssignmentMovesToInReview();
        PrimaryVisitorAssignmentRejectsMovesFromArchived();
    }
}
