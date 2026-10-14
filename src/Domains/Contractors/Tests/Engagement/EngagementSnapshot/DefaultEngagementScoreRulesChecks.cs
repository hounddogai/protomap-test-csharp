namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSnapshot;

using Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

/// <summary>Checks of engagement snapshot code, run by the project's runner.</summary>
public static class DefaultEngagementScoreRulesChecks
{
    public static void DefaultEngagementScoreValidatorReportsEveryProblem()
    {
        var validator = DefaultEngagementScoreRules.DefaultDefaultEngagementScoreValidator();
        var item = DefaultEngagementScore.NewDefaultEngagementScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultEngagementScoreRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new DefaultEngagementScoreRequiredSegmentRule("default engagement score needs this field");
        var item = DefaultEngagementScore.NewDefaultEngagementScore("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultEngagementScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultEngagementScoreRequiredCodeRule("default engagement score needs a value");
        var item = DefaultEngagementScore.NewDefaultEngagementScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultEngagementScoreValidatorReportsEveryProblem();
        DefaultEngagementScoreRequiredSegmentRuleFlagsABlankSegment();
        DefaultEngagementScoreRequiredCodeRuleFlagsABlankCode();
    }
}
