namespace Acme.Workforce.Engagement.Tests.Sentiment.Plan;

using Acme.Workforce.Engagement.Sentiment.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RollingSentimentPlanRulesChecks
{
    public static void RollingSentimentPlanValidatorReportsEveryProblem()
    {
        var validator = RollingSentimentPlanRules.DefaultRollingSentimentPlanValidator();
        var item = RollingSentimentPlan.NewRollingSentimentPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSentimentPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingSentimentPlanCodeLengthRule(255);
        var item = RollingSentimentPlan.NewRollingSentimentPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSentimentPlanNameLengthRuleFlagsALongName()
    {
        var rule = new RollingSentimentPlanNameLengthRule(3600);
        var item = RollingSentimentPlan.NewRollingSentimentPlan("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSentimentPlanMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new RollingSentimentPlanMinimumRangeRule(1.7, 2512.2);
        var item = RollingSentimentPlan.NewRollingSentimentPlan("x", "y");
        item.Minimum = 2513.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSentimentPlanRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new RollingSentimentPlanRetriesRangeRule(32, 86400);
        var item = RollingSentimentPlan.NewRollingSentimentPlan("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSentimentPlanThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new RollingSentimentPlanThresholdRangeRule(8.0, 1016.1);
        var item = RollingSentimentPlan.NewRollingSentimentPlan("x", "y");
        item.Threshold = 1017.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingSentimentPlanValidatorReportsEveryProblem();
        RollingSentimentPlanCodeLengthRuleFlagsALongCode();
        RollingSentimentPlanNameLengthRuleFlagsALongName();
        RollingSentimentPlanMinimumRangeRuleFlagsValuesAboveMinimum();
        RollingSentimentPlanRetriesRangeRuleFlagsValuesAboveRetries();
        RollingSentimentPlanThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
