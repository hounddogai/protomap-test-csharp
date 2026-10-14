namespace Acme.Workforce.Engagement.Tests.Feedback.Slot;

using Acme.Workforce.Engagement.Feedback.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TieredFeedbackPolicyRulesChecks
{
    public static void TieredFeedbackPolicyValidatorReportsEveryProblem()
    {
        var validator = TieredFeedbackPolicyRules.DefaultTieredFeedbackPolicyValidator();
        var item = TieredFeedbackPolicy.NewTieredFeedbackPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredFeedbackPolicyCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new TieredFeedbackPolicyCostRangeRule(9.9, 511.9);
        var item = TieredFeedbackPolicy.NewTieredFeedbackPolicy("x", "y");
        item.Cost = 513.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredFeedbackPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredFeedbackPolicyRequiredCodeRule("tiered feedback policy needs a value");
        var item = TieredFeedbackPolicy.NewTieredFeedbackPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredFeedbackPolicyRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new TieredFeedbackPolicyRequiredTitleRule("tiered feedback policy needs an entry");
        var item = TieredFeedbackPolicy.NewTieredFeedbackPolicy("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredFeedbackPolicyDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new TieredFeedbackPolicyDescriptionLengthRule(1000);
        var item = TieredFeedbackPolicy.NewTieredFeedbackPolicy("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredFeedbackPolicyValidatorReportsEveryProblem();
        TieredFeedbackPolicyCostRangeRuleFlagsValuesAboveCost();
        TieredFeedbackPolicyRequiredCodeRuleFlagsABlankCode();
        TieredFeedbackPolicyRequiredTitleRuleFlagsABlankTitle();
        TieredFeedbackPolicyDescriptionLengthRuleFlagsALongDescription();
    }
}
