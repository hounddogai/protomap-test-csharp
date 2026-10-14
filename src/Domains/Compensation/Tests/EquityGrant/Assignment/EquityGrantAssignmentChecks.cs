namespace Acme.Workforce.Compensation.Tests.EquityGrant.Assignment;

using Acme.Workforce.Compensation.EquityGrant.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EquityGrantAssignmentChecks
{
    public static void EquityGrantAssignmentStartsInOpen()
    {
        var item = EquityGrantAssignment.NewEquityGrantAssignment("code-328", "name-517");
        Check.Equal(item.Status, EquityGrantAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantAssignmentMovesToInReview()
    {
        var item = EquityGrantAssignment.NewEquityGrantAssignment("code-328", "name-517");
        try
        {
            item.MoveTo(EquityGrantAssignmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantAssignmentStatus.InReview, "status");
    }

    public static void EquityGrantAssignmentRejectsMovesFromArchived()
    {
        var item = EquityGrantAssignment.NewEquityGrantAssignment("code-328", "name-517");
        item.Status = EquityGrantAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(EquityGrantAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        EquityGrantAssignmentStartsInOpen();
        EquityGrantAssignmentMovesToInReview();
        EquityGrantAssignmentRejectsMovesFromArchived();
    }
}
