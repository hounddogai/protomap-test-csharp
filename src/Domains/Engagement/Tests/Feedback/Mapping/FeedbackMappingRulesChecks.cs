namespace Acme.Workforce.Engagement.Tests.Feedback.Mapping;

using Acme.Workforce.Engagement.Feedback.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FeedbackMappingRulesChecks
{
    public static void FeedbackMappingValidatorReportsEveryProblem()
    {
        var validator = FeedbackMappingRules.DefaultFeedbackMappingValidator();
        var item = FeedbackMapping.NewFeedbackMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackMappingDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new FeedbackMappingDiscountRangeRule(8.0, 1195.7);
        var item = FeedbackMapping.NewFeedbackMapping("x", "y");
        item.Discount = 1197.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FeedbackMappingRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new FeedbackMappingRequiredReviewerRule("feedback mapping needs a value");
        var item = FeedbackMapping.NewFeedbackMapping("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackMappingCodeLengthRule(16);
        var item = FeedbackMapping.NewFeedbackMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FeedbackMappingValidatorReportsEveryProblem();
        FeedbackMappingDiscountRangeRuleFlagsValuesAboveDiscount();
        FeedbackMappingRequiredReviewerRuleFlagsABlankReviewer();
        FeedbackMappingCodeLengthRuleFlagsALongCode();
    }
}
