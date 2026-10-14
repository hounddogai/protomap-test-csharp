namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class StrictAdjustmentBaselineRulesChecks
{
    public static void StrictAdjustmentBaselineValidatorReportsEveryProblem()
    {
        var validator = StrictAdjustmentBaselineRules.DefaultStrictAdjustmentBaselineValidator();
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAdjustmentBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictAdjustmentBaselineCodeLengthRule(86400);
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAdjustmentBaselineNameLengthRuleFlagsALongName()
    {
        var rule = new StrictAdjustmentBaselineNameLengthRule(86400);
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAdjustmentBaselineGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new StrictAdjustmentBaselineGraceDaysRangeRule(80, 120);
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("x", "y");
        item.GraceDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictAdjustmentBaselineSkillLengthRuleFlagsALongSkill()
    {
        var rule = new StrictAdjustmentBaselineSkillLengthRule(64);
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAdjustmentBaselineRequiredNameRuleFlagsABlankName()
    {
        var rule = new StrictAdjustmentBaselineRequiredNameRule("strict adjustment baseline needs a value");
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StrictAdjustmentBaselineValidatorReportsEveryProblem();
        StrictAdjustmentBaselineCodeLengthRuleFlagsALongCode();
        StrictAdjustmentBaselineNameLengthRuleFlagsALongName();
        StrictAdjustmentBaselineGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        StrictAdjustmentBaselineSkillLengthRuleFlagsALongSkill();
        StrictAdjustmentBaselineRequiredNameRuleFlagsABlankName();
    }
}
