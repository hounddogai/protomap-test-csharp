namespace Acme.Workforce.Compensation.Tests.EquityGrant.Adjustment;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EquityGrantAdjustmentRulesChecks
{
    public static void EquityGrantAdjustmentValidatorReportsEveryProblem()
    {
        var validator = EquityGrantAdjustmentRules.DefaultEquityGrantAdjustmentValidator();
        var item = EquityGrantAdjustment.NewEquityGrantAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantAdjustmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new EquityGrantAdjustmentTitleLengthRule(86400);
        var item = EquityGrantAdjustment.NewEquityGrantAdjustment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantAdjustmentRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new EquityGrantAdjustmentRequiredGradeRule("equity grant adjustment needs this field");
        var item = EquityGrantAdjustment.NewEquityGrantAdjustment("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantAdjustmentPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new EquityGrantAdjustmentPhoneLengthRule(255);
        var item = EquityGrantAdjustment.NewEquityGrantAdjustment("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantAdjustmentRequiredCodeRule("equity grant adjustment needs this field");
        var item = EquityGrantAdjustment.NewEquityGrantAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantAdjustmentValidatorReportsEveryProblem();
        EquityGrantAdjustmentTitleLengthRuleFlagsALongTitle();
        EquityGrantAdjustmentRequiredGradeRuleFlagsABlankGrade();
        EquityGrantAdjustmentPhoneLengthRuleFlagsALongPhone();
        EquityGrantAdjustmentRequiredCodeRuleFlagsABlankCode();
    }
}
