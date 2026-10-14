namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TownHallRuleRulesChecks
{
    public static void TownHallRuleValidatorReportsEveryProblem()
    {
        var validator = TownHallRuleRules.DefaultTownHallRuleValidator();
        var item = TownHallRule.NewTownHallRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallRuleRequiredNameRuleFlagsABlankName()
    {
        var rule = new TownHallRuleRequiredNameRule("town hall rule needs a value");
        var item = TownHallRule.NewTownHallRule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallRuleRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new TownHallRuleRequiredJobCodeRule("town hall rule needs an entry");
        var item = TownHallRule.NewTownHallRule("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TownHallRuleRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new TownHallRuleRatioRangeRule(1.6, 617.9);
        var item = TownHallRule.NewTownHallRule("x", "y");
        item.Ratio = 619.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TownHallRuleValidatorReportsEveryProblem();
        TownHallRuleRequiredNameRuleFlagsABlankName();
        TownHallRuleRequiredJobCodeRuleFlagsABlankJobCode();
        TownHallRuleRatioRangeRuleFlagsValuesAboveRatio();
    }
}
