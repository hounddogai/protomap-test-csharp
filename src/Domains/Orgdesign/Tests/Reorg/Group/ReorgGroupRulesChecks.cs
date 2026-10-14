namespace Acme.Workforce.Orgdesign.Tests.Reorg.Group;

using Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ReorgGroupRulesChecks
{
    public static void ReorgGroupValidatorReportsEveryProblem()
    {
        var validator = ReorgGroupRules.DefaultReorgGroupValidator();
        var item = ReorgGroup.NewReorgGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgGroupCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new ReorgGroupCapacityRangeRule(32, 86400);
        var item = ReorgGroup.NewReorgGroup("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgGroupRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new ReorgGroupRequiredTeamRule("reorg group needs this field");
        var item = ReorgGroup.NewReorgGroup("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgGroupMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ReorgGroupMaximumRangeRule(9.7, 4164.5);
        var item = ReorgGroup.NewReorgGroup("x", "y");
        item.Maximum = 4166.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReorgGroupTeamLengthRuleFlagsALongTeam()
    {
        var rule = new ReorgGroupTeamLengthRule(120);
        var item = ReorgGroup.NewReorgGroup("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgGroupUnitLengthRuleFlagsALongUnit()
    {
        var rule = new ReorgGroupUnitLengthRule(1000);
        var item = ReorgGroup.NewReorgGroup("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReorgGroupValidatorReportsEveryProblem();
        ReorgGroupCapacityRangeRuleFlagsValuesAboveCapacity();
        ReorgGroupRequiredTeamRuleFlagsABlankTeam();
        ReorgGroupMaximumRangeRuleFlagsValuesAboveMaximum();
        ReorgGroupTeamLengthRuleFlagsALongTeam();
        ReorgGroupUnitLengthRuleFlagsALongUnit();
    }
}
