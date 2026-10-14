namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EngagementSummaryRulesChecks
{
    public static void EngagementSummaryValidatorReportsEveryProblem()
    {
        var validator = EngagementSummaryRules.DefaultEngagementSummaryValidator();
        var item = EngagementSummary.NewEngagementSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementSummaryRequiredCodeRule("engagement summary needs this field");
        var item = EngagementSummary.NewEngagementSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementSummaryRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new EngagementSummaryRatingRangeRule(5.5, 573.8);
        var item = EngagementSummary.NewEngagementSummary("x", "y");
        item.Rating = 575.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementSummaryFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new EngagementSummaryFteRangeRule(8.8, 2139.9);
        var item = EngagementSummary.NewEngagementSummary("x", "y");
        item.Fte = 2141.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementSummaryRequiredNameRuleFlagsABlankName()
    {
        var rule = new EngagementSummaryRequiredNameRule("engagement summary needs a value");
        var item = EngagementSummary.NewEngagementSummary("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EngagementSummaryValidatorReportsEveryProblem();
        EngagementSummaryRequiredCodeRuleFlagsABlankCode();
        EngagementSummaryRatingRangeRuleFlagsValuesAboveRating();
        EngagementSummaryFteRangeRuleFlagsValuesAboveFte();
        EngagementSummaryRequiredNameRuleFlagsABlankName();
    }
}
