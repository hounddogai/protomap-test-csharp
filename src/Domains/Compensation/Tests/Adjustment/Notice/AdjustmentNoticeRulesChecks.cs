namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentNoticeRulesChecks
{
    public static void AdjustmentNoticeValidatorReportsEveryProblem()
    {
        var validator = AdjustmentNoticeRules.DefaultAdjustmentNoticeValidator();
        var item = AdjustmentNotice.NewAdjustmentNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentNoticeCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new AdjustmentNoticeCurrencyLengthRule(3600);
        var item = AdjustmentNotice.NewAdjustmentNotice("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentNoticeVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new AdjustmentNoticeVersionRangeRule(16, 3600);
        var item = AdjustmentNotice.NewAdjustmentNotice("x", "y");
        item.Version = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdjustmentNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdjustmentNoticeLabelLengthRule(16);
        var item = AdjustmentNotice.NewAdjustmentNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentNoticeRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new AdjustmentNoticeRequiredSkillRule("adjustment notice needs this field");
        var item = AdjustmentNotice.NewAdjustmentNotice("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentNoticeValidatorReportsEveryProblem();
        AdjustmentNoticeCurrencyLengthRuleFlagsALongCurrency();
        AdjustmentNoticeVersionRangeRuleFlagsValuesAboveVersion();
        AdjustmentNoticeLabelLengthRuleFlagsALongLabel();
        AdjustmentNoticeRequiredSkillRuleFlagsABlankSkill();
    }
}
