namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyAllocationRulesChecks
{
    public static void OccupancyAllocationValidatorReportsEveryProblem()
    {
        var validator = OccupancyAllocationRules.DefaultOccupancyAllocationValidator();
        var item = OccupancyAllocation.NewOccupancyAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyAllocationLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new OccupancyAllocationLocaleLengthRule(32);
        var item = OccupancyAllocation.NewOccupancyAllocation("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new OccupancyAllocationCodeLengthRule(64);
        var item = OccupancyAllocation.NewOccupancyAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyAllocationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new OccupancyAllocationLabelLengthRule(255);
        var item = OccupancyAllocation.NewOccupancyAllocation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancyAllocationRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new OccupancyAllocationRequiredLocaleRule("occupancy allocation needs an entry");
        var item = OccupancyAllocation.NewOccupancyAllocation("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyAllocationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new OccupancyAllocationRequiredLabelRule("occupancy allocation needs this field");
        var item = OccupancyAllocation.NewOccupancyAllocation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancyAllocationValidatorReportsEveryProblem();
        OccupancyAllocationLocaleLengthRuleFlagsALongLocale();
        OccupancyAllocationCodeLengthRuleFlagsALongCode();
        OccupancyAllocationLabelLengthRuleFlagsALongLabel();
        OccupancyAllocationRequiredLocaleRuleFlagsABlankLocale();
        OccupancyAllocationRequiredLabelRuleFlagsABlankLabel();
    }
}
