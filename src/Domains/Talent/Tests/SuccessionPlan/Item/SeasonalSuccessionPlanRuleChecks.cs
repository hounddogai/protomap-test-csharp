namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Item;

using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanRuleChecks
{
    public static void SeasonalSuccessionPlanRuleStartsInDraft()
    {
        var item = SeasonalSuccessionPlanRule.NewSeasonalSuccessionPlanRule("code-48", "title-21");
        Check.Equal(item.Status, SeasonalSuccessionPlanRuleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalSuccessionPlanRuleMovesToActive()
    {
        var item = SeasonalSuccessionPlanRule.NewSeasonalSuccessionPlanRule("code-48", "title-21");
        try
        {
            item.MoveTo(SeasonalSuccessionPlanRuleStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalSuccessionPlanRuleStatus.Active, "status");
    }

    public static void SeasonalSuccessionPlanRuleRejectsMovesFromClosed()
    {
        var item = SeasonalSuccessionPlanRule.NewSeasonalSuccessionPlanRule("code-48", "title-21");
        item.Status = SeasonalSuccessionPlanRuleStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalSuccessionPlanRuleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanRuleStartsInDraft();
        SeasonalSuccessionPlanRuleMovesToActive();
        SeasonalSuccessionPlanRuleRejectsMovesFromClosed();
    }
}
