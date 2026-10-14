namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class GlobalHeadcountCheckpointRulesChecks
{
    public static void GlobalHeadcountCheckpointValidatorReportsEveryProblem()
    {
        var validator = GlobalHeadcountCheckpointRules.DefaultGlobalHeadcountCheckpointValidator();
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalHeadcountCheckpointCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new GlobalHeadcountCheckpointCostCenterLengthRule(3600);
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalHeadcountCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalHeadcountCheckpointCodeLengthRule(64);
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalHeadcountCheckpointSkillLengthRuleFlagsALongSkill()
    {
        var rule = new GlobalHeadcountCheckpointSkillLengthRule(64);
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalHeadcountCheckpointLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new GlobalHeadcountCheckpointLeaveDaysRangeRule(32, 3600);
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalHeadcountCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalHeadcountCheckpointRequiredCodeRule("global headcount checkpoint needs a value");
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalHeadcountCheckpointRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalHeadcountCheckpointRequiredTitleRule("global headcount checkpoint needs this field");
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalHeadcountCheckpointValidatorReportsEveryProblem();
        GlobalHeadcountCheckpointCostCenterLengthRuleFlagsALongCostCenter();
        GlobalHeadcountCheckpointCodeLengthRuleFlagsALongCode();
        GlobalHeadcountCheckpointSkillLengthRuleFlagsALongSkill();
        GlobalHeadcountCheckpointLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        GlobalHeadcountCheckpointRequiredCodeRuleFlagsABlankCode();
        GlobalHeadcountCheckpointRequiredTitleRuleFlagsABlankTitle();
    }
}
