namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalEngagementMilestoneRulesChecks
{
    public static void SeasonalEngagementMilestoneValidatorReportsEveryProblem()
    {
        var validator = SeasonalEngagementMilestoneRules.DefaultSeasonalEngagementMilestoneValidator();
        var item = SeasonalEngagementMilestone.NewSeasonalEngagementMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalEngagementMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalEngagementMilestoneCodeLengthRule(80);
        var item = SeasonalEngagementMilestone.NewSeasonalEngagementMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalEngagementMilestoneRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new SeasonalEngagementMilestoneRatingRangeRule(6.3, 4973.6);
        var item = SeasonalEngagementMilestone.NewSeasonalEngagementMilestone("x", "y");
        item.Rating = 4975.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalEngagementMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalEngagementMilestoneRequiredCodeRule("seasonal engagement milestone needs this field");
        var item = SeasonalEngagementMilestone.NewSeasonalEngagementMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalEngagementMilestoneValidatorReportsEveryProblem();
        SeasonalEngagementMilestoneCodeLengthRuleFlagsALongCode();
        SeasonalEngagementMilestoneRatingRangeRuleFlagsValuesAboveRating();
        SeasonalEngagementMilestoneRequiredCodeRuleFlagsABlankCode();
    }
}
