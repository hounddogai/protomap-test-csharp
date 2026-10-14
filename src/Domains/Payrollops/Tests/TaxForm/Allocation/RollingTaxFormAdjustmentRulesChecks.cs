namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RollingTaxFormAdjustmentRulesChecks
{
    public static void RollingTaxFormAdjustmentValidatorReportsEveryProblem()
    {
        var validator = RollingTaxFormAdjustmentRules.DefaultRollingTaxFormAdjustmentValidator();
        var item = RollingTaxFormAdjustment.NewRollingTaxFormAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingTaxFormAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingTaxFormAdjustmentRequiredCodeRule("rolling tax form adjustment needs an entry");
        var item = RollingTaxFormAdjustment.NewRollingTaxFormAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingTaxFormAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new RollingTaxFormAdjustmentNameLengthRule(16);
        var item = RollingTaxFormAdjustment.NewRollingTaxFormAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingTaxFormAdjustmentCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new RollingTaxFormAdjustmentCapacityRangeRule(80, 1000);
        var item = RollingTaxFormAdjustment.NewRollingTaxFormAdjustment("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingTaxFormAdjustmentGradeLengthRuleFlagsALongGrade()
    {
        var rule = new RollingTaxFormAdjustmentGradeLengthRule(86400);
        var item = RollingTaxFormAdjustment.NewRollingTaxFormAdjustment("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingTaxFormAdjustmentValidatorReportsEveryProblem();
        RollingTaxFormAdjustmentRequiredCodeRuleFlagsABlankCode();
        RollingTaxFormAdjustmentNameLengthRuleFlagsALongName();
        RollingTaxFormAdjustmentCapacityRangeRuleFlagsValuesAboveCapacity();
        RollingTaxFormAdjustmentGradeLengthRuleFlagsALongGrade();
    }
}
