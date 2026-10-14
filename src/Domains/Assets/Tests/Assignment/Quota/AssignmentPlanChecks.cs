namespace Acme.Workforce.Assets.Tests.Assignment.Quota;

using Acme.Workforce.Assets.Assignment.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssignmentPlanChecks
{
    public static void AssignmentPlanStartsInOpen()
    {
        var item = AssignmentPlan.NewAssignmentPlan("code-89", "name-693");
        Check.Equal(item.Status, AssignmentPlanStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentPlanMovesToInReview()
    {
        var item = AssignmentPlan.NewAssignmentPlan("code-89", "name-693");
        try
        {
            item.MoveTo(AssignmentPlanStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentPlanStatus.InReview, "status");
    }

    public static void AssignmentPlanRejectsMovesFromArchived()
    {
        var item = AssignmentPlan.NewAssignmentPlan("code-89", "name-693");
        item.Status = AssignmentPlanStatus.Archived;
        Check.Fails(() => item.MoveTo(AssignmentPlanStatus.Open), "move");
    }

    public static void RunAll()
    {
        AssignmentPlanStartsInOpen();
        AssignmentPlanMovesToInReview();
        AssignmentPlanRejectsMovesFromArchived();
    }
}
