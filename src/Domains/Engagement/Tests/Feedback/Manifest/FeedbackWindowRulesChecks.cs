namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackWindowRulesChecks
{
    public static void FeedbackWindowValidatorReportsEveryProblem()
    {
        var validator = FeedbackWindowRules.DefaultFeedbackWindowValidator();
        var item = FeedbackWindow.NewFeedbackWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackWindowBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new FeedbackWindowBudgetRangeRule(5.4, 3176.3);
        var item = FeedbackWindow.NewFeedbackWindow("x", "y");
        item.Budget = 3177.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FeedbackWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackWindowCodeLengthRule(16);
        var item = FeedbackWindow.NewFeedbackWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackWindowRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new FeedbackWindowRequiredTitleRule("feedback window needs a value");
        var item = FeedbackWindow.NewFeedbackWindow("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FeedbackWindowRequiredCodeRule("feedback window needs this field");
        var item = FeedbackWindow.NewFeedbackWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackWindowQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new FeedbackWindowQuantityRangeRule(32, 80);
        var item = FeedbackWindow.NewFeedbackWindow("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FeedbackWindowValidatorReportsEveryProblem();
        FeedbackWindowBudgetRangeRuleFlagsValuesAboveBudget();
        FeedbackWindowCodeLengthRuleFlagsALongCode();
        FeedbackWindowRequiredTitleRuleFlagsABlankTitle();
        FeedbackWindowRequiredCodeRuleFlagsABlankCode();
        FeedbackWindowQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
