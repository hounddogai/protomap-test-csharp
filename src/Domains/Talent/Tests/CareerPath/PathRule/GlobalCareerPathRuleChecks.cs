namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class GlobalCareerPathRuleChecks
{
    public static void GlobalCareerPathRuleStartsInIdle()
    {
        var item = GlobalCareerPathRule.NewGlobalCareerPathRule("code-416", "label-424");
        Check.Equal(item.Status, GlobalCareerPathRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalCareerPathRuleMovesToWarming()
    {
        var item = GlobalCareerPathRule.NewGlobalCareerPathRule("code-416", "label-424");
        try
        {
            item.MoveTo(GlobalCareerPathRuleStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalCareerPathRuleStatus.Warming, "status");
    }

    public static void GlobalCareerPathRuleRejectsMovesFromStopped()
    {
        var item = GlobalCareerPathRule.NewGlobalCareerPathRule("code-416", "label-424");
        item.Status = GlobalCareerPathRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalCareerPathRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalCareerPathRuleStartsInIdle();
        GlobalCareerPathRuleMovesToWarming();
        GlobalCareerPathRuleRejectsMovesFromStopped();
    }
}
