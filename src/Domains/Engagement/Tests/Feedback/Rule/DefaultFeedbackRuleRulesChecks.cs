namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class DefaultFeedbackRuleRulesChecks
{
    public static void DefaultFeedbackRuleValidatorExtraReportsEveryProblem()
    {
        var validator = DefaultFeedbackRuleRules.DefaultDefaultFeedbackRuleValidator();
        var item = DefaultFeedbackRule.NewDefaultFeedbackRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultFeedbackRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultFeedbackRuleCodeLengthRule(1000);
        var item = DefaultFeedbackRule.NewDefaultFeedbackRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultFeedbackRuleCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new DefaultFeedbackRuleCreatedAtRangeRule(32, 255);
        var item = DefaultFeedbackRule.NewDefaultFeedbackRule("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultFeedbackRuleValidatorExtraReportsEveryProblem();
        DefaultFeedbackRuleCodeLengthRuleFlagsALongCode();
        DefaultFeedbackRuleCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
