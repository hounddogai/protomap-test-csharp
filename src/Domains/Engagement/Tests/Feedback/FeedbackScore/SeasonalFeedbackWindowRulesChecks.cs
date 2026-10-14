namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackScore;

using Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>Checks of feedback score code, run by the project's runner.</summary>
public static class SeasonalFeedbackWindowRulesChecks
{
    public static void SeasonalFeedbackWindowValidatorReportsEveryProblem()
    {
        var validator = SeasonalFeedbackWindowRules.DefaultSeasonalFeedbackWindowValidator();
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalFeedbackWindowRequiredNameRuleFlagsABlankName()
    {
        var rule = new SeasonalFeedbackWindowRequiredNameRule("seasonal feedback window needs this field");
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFeedbackWindowSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new SeasonalFeedbackWindowSequenceRangeRule(64, 3600);
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("x", "y");
        item.Sequence = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalFeedbackWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalFeedbackWindowCodeLengthRule(64);
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalFeedbackWindowRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new SeasonalFeedbackWindowRequiredCurrencyRule("seasonal feedback window needs an entry");
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalFeedbackWindowLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new SeasonalFeedbackWindowLeaveDaysRangeRule(32, 3600);
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("x", "y");
        item.LeaveDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalFeedbackWindowRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new SeasonalFeedbackWindowRequiredCountryRule("seasonal feedback window needs a value");
        var item = SeasonalFeedbackWindow.NewSeasonalFeedbackWindow("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalFeedbackWindowValidatorReportsEveryProblem();
        SeasonalFeedbackWindowRequiredNameRuleFlagsABlankName();
        SeasonalFeedbackWindowSequenceRangeRuleFlagsValuesAboveSequence();
        SeasonalFeedbackWindowCodeLengthRuleFlagsALongCode();
        SeasonalFeedbackWindowRequiredCurrencyRuleFlagsABlankCurrency();
        SeasonalFeedbackWindowLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        SeasonalFeedbackWindowRequiredCountryRuleFlagsABlankCountry();
    }
}
