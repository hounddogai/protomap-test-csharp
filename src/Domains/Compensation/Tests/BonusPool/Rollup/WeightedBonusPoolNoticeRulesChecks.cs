namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedBonusPoolNoticeRulesChecks
{
    public static void WeightedBonusPoolNoticeValidatorReportsEveryProblem()
    {
        var validator = WeightedBonusPoolNoticeRules.DefaultWeightedBonusPoolNoticeValidator();
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedBonusPoolNoticeLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new WeightedBonusPoolNoticeLimitRangeRule(1000, 3600);
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("x", "y");
        item.Limit = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedBonusPoolNoticeMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new WeightedBonusPoolNoticeMaximumRangeRule(9.9, 3978.8);
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("x", "y");
        item.Maximum = 3980.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedBonusPoolNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedBonusPoolNoticeCodeLengthRule(32);
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedBonusPoolNoticeEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new WeightedBonusPoolNoticeEmployeeIdLengthRule(255);
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedBonusPoolNoticeRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedBonusPoolNoticeRequiredNameRule("weighted bonus pool notice needs an entry");
        var item = WeightedBonusPoolNotice.NewWeightedBonusPoolNotice("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedBonusPoolNoticeValidatorReportsEveryProblem();
        WeightedBonusPoolNoticeLimitRangeRuleFlagsValuesAboveLimit();
        WeightedBonusPoolNoticeMaximumRangeRuleFlagsValuesAboveMaximum();
        WeightedBonusPoolNoticeCodeLengthRuleFlagsALongCode();
        WeightedBonusPoolNoticeEmployeeIdLengthRuleFlagsALongEmployeeId();
        WeightedBonusPoolNoticeRequiredNameRuleFlagsABlankName();
    }
}
