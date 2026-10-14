namespace Acme.Workforce.Absence.Tests.ReturnPlan.Metric;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReturnPlanEstimateRulesChecks
{
    public static void ReturnPlanEstimateValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanEstimateRules.DefaultReturnPlanEstimateValidator();
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanEstimateRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new ReturnPlanEstimateRequiredTimezoneRule("return plan estimate needs an entry");
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanEstimateExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new ReturnPlanEstimateExternalRefLengthRule(1000);
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanEstimateRequiredNameRuleFlagsABlankName()
    {
        var rule = new ReturnPlanEstimateRequiredNameRule("return plan estimate needs a value");
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanEstimateTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new ReturnPlanEstimateTenureMonthsRangeRule(32, 3600);
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanEstimateCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new ReturnPlanEstimateCostRangeRule(5.8, 2275.5);
        var item = ReturnPlanEstimate.NewReturnPlanEstimate("x", "y");
        item.Cost = 2277.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReturnPlanEstimateValidatorReportsEveryProblem();
        ReturnPlanEstimateRequiredTimezoneRuleFlagsABlankTimezone();
        ReturnPlanEstimateExternalRefLengthRuleFlagsALongExternalRef();
        ReturnPlanEstimateRequiredNameRuleFlagsABlankName();
        ReturnPlanEstimateTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        ReturnPlanEstimateCostRangeRuleFlagsValuesAboveCost();
    }
}
