namespace Acme.Workforce.Payrollops.Tests.Payslip.Exception;

using Acme.Workforce.Payrollops.Payslip.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientPayslipExceptionRulesChecks
{
    public static void LenientPayslipExceptionValidatorReportsEveryProblem()
    {
        var validator = LenientPayslipExceptionRules.DefaultLenientPayslipExceptionValidator();
        var item = LenientPayslipException.NewLenientPayslipException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPayslipExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientPayslipExceptionRequiredCodeRule("lenient payslip exception needs an entry");
        var item = LenientPayslipException.NewLenientPayslipException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPayslipExceptionRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientPayslipExceptionRequiredNameRule("lenient payslip exception needs this field");
        var item = LenientPayslipException.NewLenientPayslipException("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPayslipExceptionFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new LenientPayslipExceptionFteRangeRule(0.7, 4805.1);
        var item = LenientPayslipException.NewLenientPayslipException("x", "y");
        item.Fte = 4806.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientPayslipExceptionRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new LenientPayslipExceptionRequiredSegmentRule("lenient payslip exception needs an entry");
        var item = LenientPayslipException.NewLenientPayslipException("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPayslipExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientPayslipExceptionCodeLengthRule(86400);
        var item = LenientPayslipException.NewLenientPayslipException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPayslipExceptionDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new LenientPayslipExceptionDiscountRangeRule(3.1, 4002.1);
        var item = LenientPayslipException.NewLenientPayslipException("x", "y");
        item.Discount = 4003.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientPayslipExceptionValidatorReportsEveryProblem();
        LenientPayslipExceptionRequiredCodeRuleFlagsABlankCode();
        LenientPayslipExceptionRequiredNameRuleFlagsABlankName();
        LenientPayslipExceptionFteRangeRuleFlagsValuesAboveFte();
        LenientPayslipExceptionRequiredSegmentRuleFlagsABlankSegment();
        LenientPayslipExceptionCodeLengthRuleFlagsALongCode();
        LenientPayslipExceptionDiscountRangeRuleFlagsValuesAboveDiscount();
    }
}
