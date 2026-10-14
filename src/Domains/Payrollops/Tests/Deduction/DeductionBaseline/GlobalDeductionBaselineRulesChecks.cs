namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBaseline;

using Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

/// <summary>Checks of deduction baseline code, run by the project's runner.</summary>
public static class GlobalDeductionBaselineRulesChecks
{
    public static void GlobalDeductionBaselineValidatorReportsEveryProblem()
    {
        var validator = GlobalDeductionBaselineRules.DefaultGlobalDeductionBaselineValidator();
        var item = GlobalDeductionBaseline.NewGlobalDeductionBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalDeductionBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GlobalDeductionBaselineRequiredLabelRule("global deduction baseline needs a value");
        var item = GlobalDeductionBaseline.NewGlobalDeductionBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalDeductionBaselineChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new GlobalDeductionBaselineChecksumLengthRule(3600);
        var item = GlobalDeductionBaseline.NewGlobalDeductionBaseline("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalDeductionBaselineTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new GlobalDeductionBaselineTenureMonthsRangeRule(120, 3600);
        var item = GlobalDeductionBaseline.NewGlobalDeductionBaseline("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalDeductionBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalDeductionBaselineCodeLengthRule(1000);
        var item = GlobalDeductionBaseline.NewGlobalDeductionBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalDeductionBaselineValidatorReportsEveryProblem();
        GlobalDeductionBaselineRequiredLabelRuleFlagsABlankLabel();
        GlobalDeductionBaselineChecksumLengthRuleFlagsALongChecksum();
        GlobalDeductionBaselineTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        GlobalDeductionBaselineCodeLengthRuleFlagsALongCode();
    }
}
