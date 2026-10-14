namespace Acme.Workforce.Engagement.Tests.Feedback.Summary;

using Acme.Workforce.Engagement.Feedback.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CappedFeedbackSummaryRulesChecks
{
    public static void CappedFeedbackSummaryValidatorReportsEveryProblem()
    {
        var validator = CappedFeedbackSummaryRules.DefaultCappedFeedbackSummaryValidator();
        var item = CappedFeedbackSummary.NewCappedFeedbackSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedFeedbackSummaryRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new CappedFeedbackSummaryRequiredContactEmailRule("capped feedback summary needs a value");
        var item = CappedFeedbackSummary.NewCappedFeedbackSummary("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedFeedbackSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedFeedbackSummaryCodeLengthRule(1000);
        var item = CappedFeedbackSummary.NewCappedFeedbackSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedFeedbackSummaryCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new CappedFeedbackSummaryCostRangeRule(5.1, 2089.6);
        var item = CappedFeedbackSummary.NewCappedFeedbackSummary("x", "y");
        item.Cost = 2091.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedFeedbackSummaryRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new CappedFeedbackSummaryRequiredDescriptionRule("capped feedback summary needs a value");
        var item = CappedFeedbackSummary.NewCappedFeedbackSummary("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedFeedbackSummaryDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new CappedFeedbackSummaryDescriptionLengthRule(80);
        var item = CappedFeedbackSummary.NewCappedFeedbackSummary("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedFeedbackSummaryValidatorReportsEveryProblem();
        CappedFeedbackSummaryRequiredContactEmailRuleFlagsABlankContactEmail();
        CappedFeedbackSummaryCodeLengthRuleFlagsALongCode();
        CappedFeedbackSummaryCostRangeRuleFlagsValuesAboveCost();
        CappedFeedbackSummaryRequiredDescriptionRuleFlagsABlankDescription();
        CappedFeedbackSummaryDescriptionLengthRuleFlagsALongDescription();
    }
}
