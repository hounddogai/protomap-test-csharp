namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanAssignmentChecks
{
    public static void SuccessionPlanAssignmentStartsInDraft()
    {
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("code-428", "label-144");
        Check.Equal(item.Status, SuccessionPlanAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanAssignmentMovesToActive()
    {
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("code-428", "label-144");
        try
        {
            item.MoveTo(SuccessionPlanAssignmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanAssignmentStatus.Active, "status");
    }

    public static void SuccessionPlanAssignmentRejectsMovesFromClosed()
    {
        var item = SuccessionPlanAssignment.NewSuccessionPlanAssignment("code-428", "label-144");
        item.Status = SuccessionPlanAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(SuccessionPlanAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanAssignmentStartsInDraft();
        SuccessionPlanAssignmentMovesToActive();
        SuccessionPlanAssignmentRejectsMovesFromClosed();
    }
}
