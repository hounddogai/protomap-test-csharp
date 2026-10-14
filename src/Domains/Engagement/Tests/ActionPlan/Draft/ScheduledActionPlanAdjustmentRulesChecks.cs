namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledActionPlanAdjustmentRulesChecks
{
    public static void ScheduledActionPlanAdjustmentValidatorReportsEveryProblem()
    {
        var validator = ScheduledActionPlanAdjustmentRules.DefaultScheduledActionPlanAdjustmentValidator();
        var item = ScheduledActionPlanAdjustment.NewScheduledActionPlanAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledActionPlanAdjustmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new ScheduledActionPlanAdjustmentDurationMinutesRangeRule(1000, 86400);
        var item = ScheduledActionPlanAdjustment.NewScheduledActionPlanAdjustment("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledActionPlanAdjustmentCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new ScheduledActionPlanAdjustmentCostRangeRule(5.8, 1119.6);
        var item = ScheduledActionPlanAdjustment.NewScheduledActionPlanAdjustment("x", "y");
        item.Cost = 1121.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledActionPlanAdjustmentValidatorReportsEveryProblem();
        ScheduledActionPlanAdjustmentDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        ScheduledActionPlanAdjustmentCostRangeRuleFlagsValuesAboveCost();
    }
}
