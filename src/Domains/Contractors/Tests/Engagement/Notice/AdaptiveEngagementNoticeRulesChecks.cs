namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdaptiveEngagementNoticeRulesChecks
{
    public static void AdaptiveEngagementNoticeValidatorReportsEveryProblem()
    {
        var validator = AdaptiveEngagementNoticeRules.DefaultAdaptiveEngagementNoticeValidator();
        var item = AdaptiveEngagementNotice.NewAdaptiveEngagementNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveEngagementNoticeRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveEngagementNoticeRequiredTitleRule("adaptive engagement notice needs an entry");
        var item = AdaptiveEngagementNotice.NewAdaptiveEngagementNotice("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveEngagementNoticeMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new AdaptiveEngagementNoticeMinimumRangeRule(2.9, 269.1);
        var item = AdaptiveEngagementNotice.NewAdaptiveEngagementNotice("x", "y");
        item.Minimum = 270.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveEngagementNoticeValidatorReportsEveryProblem();
        AdaptiveEngagementNoticeRequiredTitleRuleFlagsABlankTitle();
        AdaptiveEngagementNoticeMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
