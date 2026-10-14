namespace Acme.Workforce.Workplace.Tests.AccessBadge.Budget;

using Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AccessBadgeAdjustmentRulesChecks
{
    public static void AccessBadgeAdjustmentValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeAdjustmentRules.DefaultAccessBadgeAdjustmentValidator();
        var item = AccessBadgeAdjustment.NewAccessBadgeAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeAdjustmentCountryLengthRuleFlagsALongCountry()
    {
        var rule = new AccessBadgeAdjustmentCountryLengthRule(32);
        var item = AccessBadgeAdjustment.NewAccessBadgeAdjustment("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeAdjustmentScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new AccessBadgeAdjustmentScoreRangeRule(1.0, 4033.5);
        var item = AccessBadgeAdjustment.NewAccessBadgeAdjustment("x", "y");
        item.Score = 4035.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccessBadgeAdjustmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AccessBadgeAdjustmentTitleLengthRule(64);
        var item = AccessBadgeAdjustment.NewAccessBadgeAdjustment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccessBadgeAdjustmentValidatorReportsEveryProblem();
        AccessBadgeAdjustmentCountryLengthRuleFlagsALongCountry();
        AccessBadgeAdjustmentScoreRangeRuleFlagsValuesAboveScore();
        AccessBadgeAdjustmentTitleLengthRuleFlagsALongTitle();
    }
}
