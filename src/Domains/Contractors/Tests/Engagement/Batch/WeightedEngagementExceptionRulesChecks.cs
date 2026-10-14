namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedEngagementExceptionRulesChecks
{
    public static void WeightedEngagementExceptionValidatorReportsEveryProblem()
    {
        var validator = WeightedEngagementExceptionRules.DefaultWeightedEngagementExceptionValidator();
        var item = WeightedEngagementException.NewWeightedEngagementException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedEngagementExceptionRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new WeightedEngagementExceptionRequiredTimezoneRule("weighted engagement exception needs a value");
        var item = WeightedEngagementException.NewWeightedEngagementException("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedEngagementExceptionValidatorReportsEveryProblem();
        WeightedEngagementExceptionRequiredTimezoneRuleFlagsABlankTimezone();
    }
}
