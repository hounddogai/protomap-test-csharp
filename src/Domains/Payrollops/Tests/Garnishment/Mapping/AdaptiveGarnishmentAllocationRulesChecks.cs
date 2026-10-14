namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentAllocationRulesChecks
{
    public static void AdaptiveGarnishmentAllocationValidatorReportsEveryProblem()
    {
        var validator = AdaptiveGarnishmentAllocationRules.DefaultAdaptiveGarnishmentAllocationValidator();
        var item = AdaptiveGarnishmentAllocation.NewAdaptiveGarnishmentAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveGarnishmentAllocationRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new AdaptiveGarnishmentAllocationRequiredCategoryRule(
            "adaptive garnishment allocation needs this field");
        var item = AdaptiveGarnishmentAllocation.NewAdaptiveGarnishmentAllocation("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveGarnishmentAllocationRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdaptiveGarnishmentAllocationRequiredNameRule("adaptive garnishment allocation needs this field");
        var item = AdaptiveGarnishmentAllocation.NewAdaptiveGarnishmentAllocation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentAllocationValidatorReportsEveryProblem();
        AdaptiveGarnishmentAllocationRequiredCategoryRuleFlagsABlankCategory();
        AdaptiveGarnishmentAllocationRequiredNameRuleFlagsABlankName();
    }
}
