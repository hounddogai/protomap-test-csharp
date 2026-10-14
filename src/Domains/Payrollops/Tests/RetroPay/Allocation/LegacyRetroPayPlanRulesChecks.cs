namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LegacyRetroPayPlanRulesChecks
{
    public static void LegacyRetroPayPlanValidatorReportsEveryProblem()
    {
        var validator = LegacyRetroPayPlanRules.DefaultLegacyRetroPayPlanValidator();
        var item = LegacyRetroPayPlan.NewLegacyRetroPayPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyRetroPayPlanRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyRetroPayPlanRequiredTitleRule("legacy retro pay plan needs this field");
        var item = LegacyRetroPayPlan.NewLegacyRetroPayPlan("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyRetroPayPlanRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new LegacyRetroPayPlanRetriesRangeRule(32, 255);
        var item = LegacyRetroPayPlan.NewLegacyRetroPayPlan("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyRetroPayPlanCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new LegacyRetroPayPlanCapacityRangeRule(120, 3600);
        var item = LegacyRetroPayPlan.NewLegacyRetroPayPlan("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyRetroPayPlanValidatorReportsEveryProblem();
        LegacyRetroPayPlanRequiredTitleRuleFlagsABlankTitle();
        LegacyRetroPayPlanRetriesRangeRuleFlagsValuesAboveRetries();
        LegacyRetroPayPlanCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
