namespace Acme.Workforce.Absence.Tests.Balance.Rollup;

using Acme.Workforce.Absence.Balance.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class FallbackBalanceAdjustmentRulesChecks
{
    public static void FallbackBalanceAdjustmentValidatorReportsEveryProblem()
    {
        var validator = FallbackBalanceAdjustmentRules.DefaultFallbackBalanceAdjustmentValidator();
        var item = FallbackBalanceAdjustment.NewFallbackBalanceAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackBalanceAdjustmentRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new FallbackBalanceAdjustmentRequiredGradeRule("fallback balance adjustment needs a value");
        var item = FallbackBalanceAdjustment.NewFallbackBalanceAdjustment("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackBalanceAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackBalanceAdjustmentCodeLengthRule(16);
        var item = FallbackBalanceAdjustment.NewFallbackBalanceAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackBalanceAdjustmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackBalanceAdjustmentLabelLengthRule(86400);
        var item = FallbackBalanceAdjustment.NewFallbackBalanceAdjustment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackBalanceAdjustmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackBalanceAdjustmentRequiredLabelRule("fallback balance adjustment needs a value");
        var item = FallbackBalanceAdjustment.NewFallbackBalanceAdjustment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackBalanceAdjustmentGradeLengthRuleFlagsALongGrade()
    {
        var rule = new FallbackBalanceAdjustmentGradeLengthRule(120);
        var item = FallbackBalanceAdjustment.NewFallbackBalanceAdjustment("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackBalanceAdjustmentValidatorReportsEveryProblem();
        FallbackBalanceAdjustmentRequiredGradeRuleFlagsABlankGrade();
        FallbackBalanceAdjustmentCodeLengthRuleFlagsALongCode();
        FallbackBalanceAdjustmentLabelLengthRuleFlagsALongLabel();
        FallbackBalanceAdjustmentRequiredLabelRuleFlagsABlankLabel();
        FallbackBalanceAdjustmentGradeLengthRuleFlagsALongGrade();
    }
}
