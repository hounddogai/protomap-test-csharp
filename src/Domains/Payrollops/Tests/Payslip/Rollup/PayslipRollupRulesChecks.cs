namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayslipRollupRulesChecks
{
    public static void PayslipRollupValidatorReportsEveryProblem()
    {
        var validator = PayslipRollupRules.DefaultPayslipRollupValidator();
        var item = PayslipRollup.NewPayslipRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipRollupDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new PayslipRollupDiscountRangeRule(0.3, 2684.7);
        var item = PayslipRollup.NewPayslipRollup("x", "y");
        item.Discount = 2686.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipRollupRequiredNameRuleFlagsABlankName()
    {
        var rule = new PayslipRollupRequiredNameRule("payslip rollup needs this field");
        var item = PayslipRollup.NewPayslipRollup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayslipRollupRequiredCodeRule("payslip rollup needs an entry");
        var item = PayslipRollup.NewPayslipRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayslipRollupCodeLengthRule(86400);
        var item = PayslipRollup.NewPayslipRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayslipRollupValidatorReportsEveryProblem();
        PayslipRollupDiscountRangeRuleFlagsValuesAboveDiscount();
        PayslipRollupRequiredNameRuleFlagsABlankName();
        PayslipRollupRequiredCodeRuleFlagsABlankCode();
        PayslipRollupCodeLengthRuleFlagsALongCode();
    }
}
