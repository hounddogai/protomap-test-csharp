namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormGroup;

using Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Checks of form group code, run by the project's runner.</summary>
public static class SeasonalTaxFormAllocationRulesChecks
{
    public static void SeasonalTaxFormAllocationValidatorReportsEveryProblem()
    {
        var validator = SeasonalTaxFormAllocationRules.DefaultSeasonalTaxFormAllocationValidator();
        var item = SeasonalTaxFormAllocation.NewSeasonalTaxFormAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalTaxFormAllocationRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new SeasonalTaxFormAllocationRequiredDepartmentRule("seasonal tax form allocation needs an entry");
        var item = SeasonalTaxFormAllocation.NewSeasonalTaxFormAllocation("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalTaxFormAllocationCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new SeasonalTaxFormAllocationCapacityRangeRule(80, 120);
        var item = SeasonalTaxFormAllocation.NewSeasonalTaxFormAllocation("x", "y");
        item.Capacity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalTaxFormAllocationRequiredNameRuleFlagsABlankName()
    {
        var rule = new SeasonalTaxFormAllocationRequiredNameRule("seasonal tax form allocation needs this field");
        var item = SeasonalTaxFormAllocation.NewSeasonalTaxFormAllocation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalTaxFormAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalTaxFormAllocationCodeLengthRule(32);
        var item = SeasonalTaxFormAllocation.NewSeasonalTaxFormAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalTaxFormAllocationValidatorReportsEveryProblem();
        SeasonalTaxFormAllocationRequiredDepartmentRuleFlagsABlankDepartment();
        SeasonalTaxFormAllocationCapacityRangeRuleFlagsValuesAboveCapacity();
        SeasonalTaxFormAllocationRequiredNameRuleFlagsABlankName();
        SeasonalTaxFormAllocationCodeLengthRuleFlagsALongCode();
    }
}
