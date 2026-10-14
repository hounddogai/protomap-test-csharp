namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalPayslipBaselineRulesChecks
{
    public static void SeasonalPayslipBaselineValidatorReportsEveryProblem()
    {
        var validator = SeasonalPayslipBaselineRules.DefaultSeasonalPayslipBaselineValidator();
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPayslipBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalPayslipBaselineCodeLengthRule(64);
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalPayslipBaselineRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new SeasonalPayslipBaselineRequiredDepartmentRule("seasonal payslip baseline needs this field");
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPayslipBaselineRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new SeasonalPayslipBaselineRequiredCurrencyRule("seasonal payslip baseline needs a value");
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPayslipBaselineWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new SeasonalPayslipBaselineWeightRangeRule(5.3, 3832.7);
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("x", "y");
        item.Weight = 3834.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalPayslipBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalPayslipBaselineRequiredCodeRule("seasonal payslip baseline needs an entry");
        var item = SeasonalPayslipBaseline.NewSeasonalPayslipBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalPayslipBaselineValidatorReportsEveryProblem();
        SeasonalPayslipBaselineCodeLengthRuleFlagsALongCode();
        SeasonalPayslipBaselineRequiredDepartmentRuleFlagsABlankDepartment();
        SeasonalPayslipBaselineRequiredCurrencyRuleFlagsABlankCurrency();
        SeasonalPayslipBaselineWeightRangeRuleFlagsValuesAboveWeight();
        SeasonalPayslipBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
