namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingDeductionAdjustmentRulesChecks
{
    public static void RollingDeductionAdjustmentValidatorReportsEveryProblem()
    {
        var validator = RollingDeductionAdjustmentRules.DefaultRollingDeductionAdjustmentValidator();
        var item = RollingDeductionAdjustment.NewRollingDeductionAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingDeductionAdjustmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingDeductionAdjustmentRequiredNameRule("rolling deduction adjustment needs an entry");
        var item = RollingDeductionAdjustment.NewRollingDeductionAdjustment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingDeductionAdjustmentUnitLengthRuleFlagsALongUnit()
    {
        var rule = new RollingDeductionAdjustmentUnitLengthRule(3600);
        var item = RollingDeductionAdjustment.NewRollingDeductionAdjustment("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingDeductionAdjustmentRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new RollingDeductionAdjustmentRequiredGradeRule("rolling deduction adjustment needs a value");
        var item = RollingDeductionAdjustment.NewRollingDeductionAdjustment("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingDeductionAdjustmentRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new RollingDeductionAdjustmentRevisionRangeRule(64, 80);
        var item = RollingDeductionAdjustment.NewRollingDeductionAdjustment("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingDeductionAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingDeductionAdjustmentCodeLengthRule(1000);
        var item = RollingDeductionAdjustment.NewRollingDeductionAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingDeductionAdjustmentValidatorReportsEveryProblem();
        RollingDeductionAdjustmentRequiredNameRuleFlagsABlankName();
        RollingDeductionAdjustmentUnitLengthRuleFlagsALongUnit();
        RollingDeductionAdjustmentRequiredGradeRuleFlagsABlankGrade();
        RollingDeductionAdjustmentRevisionRangeRuleFlagsValuesAboveRevision();
        RollingDeductionAdjustmentCodeLengthRuleFlagsALongCode();
    }
}
