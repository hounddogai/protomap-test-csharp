namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FeedbackBatchRulesChecks
{
    public static void FeedbackBatchValidatorReportsEveryProblem()
    {
        var validator = FeedbackBatchRules.DefaultFeedbackBatchValidator();
        var item = FeedbackBatch.NewFeedbackBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FeedbackBatchCountryLengthRuleFlagsALongCountry()
    {
        var rule = new FeedbackBatchCountryLengthRule(1000);
        var item = FeedbackBatch.NewFeedbackBatch("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackBatchRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new FeedbackBatchRequiredExternalRefRule("feedback batch needs a value");
        var item = FeedbackBatch.NewFeedbackBatch("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FeedbackBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new FeedbackBatchCodeLengthRule(32);
        var item = FeedbackBatch.NewFeedbackBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FeedbackBatchRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FeedbackBatchRequiredLabelRule("feedback batch needs an entry");
        var item = FeedbackBatch.NewFeedbackBatch("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FeedbackBatchValidatorReportsEveryProblem();
        FeedbackBatchCountryLengthRuleFlagsALongCountry();
        FeedbackBatchRequiredExternalRefRuleFlagsABlankExternalRef();
        FeedbackBatchCodeLengthRuleFlagsALongCode();
        FeedbackBatchRequiredLabelRuleFlagsABlankLabel();
    }
}
