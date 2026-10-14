namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AssignmentTargetChecks
{
    public static void AssignmentTargetStartsInOpen()
    {
        var item = AssignmentTarget.NewAssignmentTarget("code-575", "label-783");
        Check.Equal(item.Status, AssignmentTargetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentTargetMovesToInReview()
    {
        var item = AssignmentTarget.NewAssignmentTarget("code-575", "label-783");
        try
        {
            item.MoveTo(AssignmentTargetStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentTargetStatus.InReview, "status");
    }

    public static void AssignmentTargetRejectsMovesFromArchived()
    {
        var item = AssignmentTarget.NewAssignmentTarget("code-575", "label-783");
        item.Status = AssignmentTargetStatus.Archived;
        Check.Fails(() => item.MoveTo(AssignmentTargetStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssignmentTargetStartsInOpen();
        AssignmentTargetMovesToInReview();
        AssignmentTargetRejectsMovesFromArchived();
    }
}
