namespace Acme.Workforce.Orgdesign.Tests.Position.Rule;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PositionRuleChecks
{
    public static void PositionRuleStartsInIdle()
    {
        var item = PositionRule.NewPositionRule("code-185", "label-752");
        Check.Equal(item.Status, PositionRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionRuleMovesToWarming()
    {
        var item = PositionRule.NewPositionRule("code-185", "label-752");
        try
        {
            item.MoveTo(PositionRuleStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionRuleStatus.Warming, "status");
    }

    public static void PositionRuleRejectsMovesFromStopped()
    {
        var item = PositionRule.NewPositionRule("code-185", "label-752");
        item.Status = PositionRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(PositionRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PositionRuleStartsInIdle();
        PositionRuleMovesToWarming();
        PositionRuleRejectsMovesFromStopped();
    }
}
