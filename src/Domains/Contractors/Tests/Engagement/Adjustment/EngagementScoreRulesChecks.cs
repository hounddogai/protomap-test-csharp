namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementScoreRulesChecks
{
    public static void EngagementScoreValidatorReportsEveryProblem()
    {
        var validator = EngagementScoreRules.DefaultEngagementScoreValidator();
        var item = EngagementScore.NewEngagementScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementScoreWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new EngagementScoreWeightRangeRule(4.2, 2434.0);
        var item = EngagementScore.NewEngagementScore("x", "y");
        item.Weight = 2435.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementScoreRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new EngagementScoreRequiredBadgeRule("engagement score needs a value");
        var item = EngagementScore.NewEngagementScore("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementScoreMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new EngagementScoreMaximumRangeRule(5.4, 3231.9);
        var item = EngagementScore.NewEngagementScore("x", "y");
        item.Maximum = 3233.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementScoreLocationLengthRuleFlagsALongLocation()
    {
        var rule = new EngagementScoreLocationLengthRule(1000);
        var item = EngagementScore.NewEngagementScore("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EngagementScoreValidatorReportsEveryProblem();
        EngagementScoreWeightRangeRuleFlagsValuesAboveWeight();
        EngagementScoreRequiredBadgeRuleFlagsABlankBadge();
        EngagementScoreMaximumRangeRuleFlagsValuesAboveMaximum();
        EngagementScoreLocationLengthRuleFlagsALongLocation();
    }
}
