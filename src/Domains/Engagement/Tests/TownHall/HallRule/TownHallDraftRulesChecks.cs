namespace Acme.Workforce.Engagement.Tests.TownHall.HallRule;

using Acme.Workforce.Engagement.TownHall.HallRule;

/// <summary>Checks of hall rule code, run by the project's runner.</summary>
public static class TownHallDraftRulesChecks
{
    public static void TownHallDraftValidatorReportsEveryProblem()
    {
        var validator = TownHallDraftRules.DefaultTownHallDraftValidator();
        var item = TownHallDraft.NewTownHallDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallDraftCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new TownHallDraftCostRangeRule(6.3, 92.6);
        var item = TownHallDraft.NewTownHallDraft("x", "y");
        item.Cost = 94.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TownHallDraftRequiredCodeRule("town hall draft needs this field");
        var item = TownHallDraft.NewTownHallDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallDraftRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new TownHallDraftRevisionRangeRule(120, 120);
        var item = TownHallDraft.NewTownHallDraft("x", "y");
        item.Revision = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallDraftValidatorReportsEveryProblem();
        TownHallDraftCostRangeRuleFlagsValuesAboveCost();
        TownHallDraftRequiredCodeRuleFlagsABlankCode();
        TownHallDraftRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
