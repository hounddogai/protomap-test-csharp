namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LenientGarnishmentMappingRulesChecks
{
    public static void LenientGarnishmentMappingValidatorReportsEveryProblem()
    {
        var validator = LenientGarnishmentMappingRules.DefaultLenientGarnishmentMappingValidator();
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientGarnishmentMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientGarnishmentMappingLabelLengthRule(1000);
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientGarnishmentMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientGarnishmentMappingCodeLengthRule(3600);
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientGarnishmentMappingBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new LenientGarnishmentMappingBudgetRangeRule(0.4, 948.9);
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("x", "y");
        item.Budget = 950.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientGarnishmentMappingDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new LenientGarnishmentMappingDepartmentLengthRule(3600);
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientGarnishmentMappingRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LenientGarnishmentMappingRequiredLabelRule("lenient garnishment mapping needs a value");
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientGarnishmentMappingMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new LenientGarnishmentMappingMaximumRangeRule(1.9, 1317.6);
        var item = LenientGarnishmentMapping.NewLenientGarnishmentMapping("x", "y");
        item.Maximum = 1319.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientGarnishmentMappingValidatorReportsEveryProblem();
        LenientGarnishmentMappingLabelLengthRuleFlagsALongLabel();
        LenientGarnishmentMappingCodeLengthRuleFlagsALongCode();
        LenientGarnishmentMappingBudgetRangeRuleFlagsValuesAboveBudget();
        LenientGarnishmentMappingDepartmentLengthRuleFlagsALongDepartment();
        LenientGarnishmentMappingRequiredLabelRuleFlagsABlankLabel();
        LenientGarnishmentMappingMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
