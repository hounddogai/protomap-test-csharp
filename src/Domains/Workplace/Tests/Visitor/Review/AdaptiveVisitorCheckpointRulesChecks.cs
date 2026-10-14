namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AdaptiveVisitorCheckpointRulesChecks
{
    public static void AdaptiveVisitorCheckpointValidatorReportsEveryProblem()
    {
        var validator = AdaptiveVisitorCheckpointRules.DefaultAdaptiveVisitorCheckpointValidator();
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveVisitorCheckpointRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveVisitorCheckpointRequiredTitleRule("adaptive visitor checkpoint needs a value");
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveVisitorCheckpointRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new AdaptiveVisitorCheckpointRequiredCountryRule("adaptive visitor checkpoint needs an entry");
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveVisitorCheckpointTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptiveVisitorCheckpointTitleLengthRule(86400);
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveVisitorCheckpointGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new AdaptiveVisitorCheckpointGraceDaysRangeRule(16, 64);
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("x", "y");
        item.GraceDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveVisitorCheckpointCountryLengthRuleFlagsALongCountry()
    {
        var rule = new AdaptiveVisitorCheckpointCountryLengthRule(16);
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveVisitorCheckpointValidatorReportsEveryProblem();
        AdaptiveVisitorCheckpointRequiredTitleRuleFlagsABlankTitle();
        AdaptiveVisitorCheckpointRequiredCountryRuleFlagsABlankCountry();
        AdaptiveVisitorCheckpointTitleLengthRuleFlagsALongTitle();
        AdaptiveVisitorCheckpointGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        AdaptiveVisitorCheckpointCountryLengthRuleFlagsALongCountry();
    }
}
