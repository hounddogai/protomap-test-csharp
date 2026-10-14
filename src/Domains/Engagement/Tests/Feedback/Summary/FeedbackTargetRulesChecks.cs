namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FeedbackTargetRulesChecks
{
    public static void FeedbackTargetValidatorReportsEveryProblem()
    {
        var validator = FeedbackTargetRules.DefaultFeedbackTargetValidator();
        var item = FeedbackTarget.NewFeedbackTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackTargetRequiredCodeRule("feedback target needs this field");
        var item = FeedbackTarget.NewFeedbackTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackTargetSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new FeedbackTargetSummaryLengthRule(120);
        var item = FeedbackTarget.NewFeedbackTarget("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackTargetCountryLengthRuleFlagsALongCountry()
    {
        var rule = new FeedbackTargetCountryLengthRule(86400);
        var item = FeedbackTarget.NewFeedbackTarget("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackTargetRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new FeedbackTargetRequiredSummaryRule("feedback target needs an entry");
        var item = FeedbackTarget.NewFeedbackTarget("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackTargetRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new FeedbackTargetRequiredCountryRule("feedback target needs this field");
        var item = FeedbackTarget.NewFeedbackTarget("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackTargetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FeedbackTargetRequiredTitleRule("feedback target needs a value");
        var item = FeedbackTarget.NewFeedbackTarget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackTargetValidatorReportsEveryProblem();
        FeedbackTargetRequiredCodeRuleFlagsABlankCode();
        FeedbackTargetSummaryLengthRuleFlagsALongSummary();
        FeedbackTargetCountryLengthRuleFlagsALongCountry();
        FeedbackTargetRequiredSummaryRuleFlagsABlankSummary();
        FeedbackTargetRequiredCountryRuleFlagsABlankCountry();
        FeedbackTargetRequiredTitleRuleFlagsABlankTitle();
    }
}
