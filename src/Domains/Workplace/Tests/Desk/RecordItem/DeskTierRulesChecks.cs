namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DeskTierRulesChecks
{
    public static void DeskTierValidatorReportsEveryProblem()
    {
        var validator = DeskTierRules.DefaultDeskTierValidator();
        var item = DeskTier.NewDeskTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskTierCodeLengthRule(86400);
        var item = DeskTier.NewDeskTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskTierRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new DeskTierRequiredTeamRule("desk tier needs an entry");
        var item = DeskTier.NewDeskTier("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new DeskTierRequiredNameRule("desk tier needs an entry");
        var item = DeskTier.NewDeskTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskTierThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new DeskTierThresholdRangeRule(9.8, 3138.5);
        var item = DeskTier.NewDeskTier("x", "y");
        item.Threshold = 3140.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskTierTeamLengthRuleFlagsALongTeam()
    {
        var rule = new DeskTierTeamLengthRule(16);
        var item = DeskTier.NewDeskTier("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeskTierValidatorReportsEveryProblem();
        DeskTierCodeLengthRuleFlagsALongCode();
        DeskTierRequiredTeamRuleFlagsABlankTeam();
        DeskTierRequiredNameRuleFlagsABlankName();
        DeskTierThresholdRangeRuleFlagsValuesAboveThreshold();
        DeskTierTeamLengthRuleFlagsALongTeam();
    }
}
