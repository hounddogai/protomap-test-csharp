namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AssignmentBaselineChecks
{
    public static void AssignmentBaselineStartsInOpen()
    {
        var item = AssignmentBaseline.NewAssignmentBaseline("code-669", "label-116");
        Check.Equal(item.Status, AssignmentBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentBaselineMovesToResolved()
    {
        var item = AssignmentBaseline.NewAssignmentBaseline("code-669", "label-116");
        try
        {
            item.MoveTo(AssignmentBaselineStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentBaselineStatus.Resolved, "status");
    }

    public static void AssignmentBaselineRejectsMovesFromArchived()
    {
        var item = AssignmentBaseline.NewAssignmentBaseline("code-669", "label-116");
        item.Status = AssignmentBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(AssignmentBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssignmentBaselineStartsInOpen();
        AssignmentBaselineMovesToResolved();
        AssignmentBaselineRejectsMovesFromArchived();
    }
}
