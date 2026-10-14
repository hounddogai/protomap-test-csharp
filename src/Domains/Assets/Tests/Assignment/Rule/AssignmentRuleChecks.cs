namespace Acme.Workforce.Assets.Tests.Assignment.Rule;

using Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AssignmentRuleChecks
{
    public static void AssignmentRuleStartsInIdle()
    {
        var item = AssignmentRule.NewAssignmentRule("code-37", "title-585");
        Check.Equal(item.Status, AssignmentRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentRuleMovesToWarming()
    {
        var item = AssignmentRule.NewAssignmentRule("code-37", "title-585");
        try
        {
            item.MoveTo(AssignmentRuleStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentRuleStatus.Warming, "status");
    }

    public static void AssignmentRuleRejectsMovesFromStopped()
    {
        var item = AssignmentRule.NewAssignmentRule("code-37", "title-585");
        item.Status = AssignmentRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(AssignmentRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AssignmentRuleStartsInIdle();
        AssignmentRuleMovesToWarming();
        AssignmentRuleRejectsMovesFromStopped();
    }
}
