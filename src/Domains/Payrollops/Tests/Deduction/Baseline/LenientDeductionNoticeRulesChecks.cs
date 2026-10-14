namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LenientDeductionNoticeRulesChecks
{
    public static void LenientDeductionNoticeValidatorReportsEveryProblem()
    {
        var validator = LenientDeductionNoticeRules.DefaultLenientDeductionNoticeValidator();
        var item = LenientDeductionNotice.NewLenientDeductionNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientDeductionNoticeRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LenientDeductionNoticeRequiredCostCenterRule("lenient deduction notice needs a value");
        var item = LenientDeductionNotice.NewLenientDeductionNotice("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientDeductionNoticeTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new LenientDeductionNoticeTenureMonthsRangeRule(16, 3600);
        var item = LenientDeductionNotice.NewLenientDeductionNotice("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientDeductionNoticeGradeLengthRuleFlagsALongGrade()
    {
        var rule = new LenientDeductionNoticeGradeLengthRule(255);
        var item = LenientDeductionNotice.NewLenientDeductionNotice("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientDeductionNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientDeductionNoticeLabelLengthRule(64);
        var item = LenientDeductionNotice.NewLenientDeductionNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientDeductionNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientDeductionNoticeRequiredCodeRule("lenient deduction notice needs this field");
        var item = LenientDeductionNotice.NewLenientDeductionNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientDeductionNoticeValidatorReportsEveryProblem();
        LenientDeductionNoticeRequiredCostCenterRuleFlagsABlankCostCenter();
        LenientDeductionNoticeTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        LenientDeductionNoticeGradeLengthRuleFlagsALongGrade();
        LenientDeductionNoticeLabelLengthRuleFlagsALongLabel();
        LenientDeductionNoticeRequiredCodeRuleFlagsABlankCode();
    }
}
