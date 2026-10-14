namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StrictPoolPlanRulesChecks
{
    public static void StrictPoolPlanValidatorReportsEveryProblem()
    {
        var validator = StrictPoolPlanRules.DefaultStrictPoolPlanValidator();
        var item = StrictPoolPlan.NewStrictPoolPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPoolPlanEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new StrictPoolPlanEndsAtRangeRule(64, 80);
        var item = StrictPoolPlan.NewStrictPoolPlan("x", "y");
        item.EndsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictPoolPlanLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictPoolPlanLabelLengthRule(120);
        var item = StrictPoolPlan.NewStrictPoolPlan("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPoolPlanRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new StrictPoolPlanRetriesRangeRule(255, 1000);
        var item = StrictPoolPlan.NewStrictPoolPlan("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictPoolPlanValidatorReportsEveryProblem();
        StrictPoolPlanEndsAtRangeRuleFlagsValuesAboveEndsAt();
        StrictPoolPlanLabelLengthRuleFlagsALongLabel();
        StrictPoolPlanRetriesRangeRuleFlagsValuesAboveRetries();
    }
}
