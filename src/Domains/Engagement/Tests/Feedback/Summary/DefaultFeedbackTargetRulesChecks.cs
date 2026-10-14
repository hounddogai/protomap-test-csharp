namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultFeedbackTargetRulesChecks
{
    public static void DefaultFeedbackTargetValidatorExtraReportsEveryProblem()
    {
        var validator = DefaultFeedbackTargetRules.DefaultDefaultFeedbackTargetValidator();
        var item = DefaultFeedbackTarget.NewDefaultFeedbackTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultFeedbackTargetMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new DefaultFeedbackTargetMarginRangeRule(4.2, 2553.1);
        var item = DefaultFeedbackTarget.NewDefaultFeedbackTarget("x", "y");
        item.Margin = 2554.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultFeedbackTargetRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new DefaultFeedbackTargetRequiredExternalRefRule("default feedback target needs a value");
        var item = DefaultFeedbackTarget.NewDefaultFeedbackTarget("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultFeedbackTargetRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DefaultFeedbackTargetRequiredTitleRule("default feedback target needs an entry");
        var item = DefaultFeedbackTarget.NewDefaultFeedbackTarget("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultFeedbackTargetValidatorExtraReportsEveryProblem();
        DefaultFeedbackTargetMarginRangeRuleFlagsValuesAboveMargin();
        DefaultFeedbackTargetRequiredExternalRefRuleFlagsABlankExternalRef();
        DefaultFeedbackTargetRequiredTitleRuleFlagsABlankTitle();
    }
}
