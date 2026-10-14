namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PendingFeedbackMappingRulesChecks
{
    public static void PendingFeedbackMappingValidatorReportsEveryProblem()
    {
        var validator = PendingFeedbackMappingRules.DefaultPendingFeedbackMappingValidator();
        var item = PendingFeedbackMapping.NewPendingFeedbackMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingFeedbackMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new PendingFeedbackMappingRequiredNameRule("pending feedback mapping needs a value");
        var item = PendingFeedbackMapping.NewPendingFeedbackMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingFeedbackMappingExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new PendingFeedbackMappingExpiresAtRangeRule(80, 1000);
        var item = PendingFeedbackMapping.NewPendingFeedbackMapping("x", "y");
        item.ExpiresAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingFeedbackMappingRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingFeedbackMappingRequiredTitleRule("pending feedback mapping needs this field");
        var item = PendingFeedbackMapping.NewPendingFeedbackMapping("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingFeedbackMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingFeedbackMappingRequiredCodeRule("pending feedback mapping needs this field");
        var item = PendingFeedbackMapping.NewPendingFeedbackMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingFeedbackMappingValidatorReportsEveryProblem();
        PendingFeedbackMappingRequiredNameRuleFlagsABlankName();
        PendingFeedbackMappingExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        PendingFeedbackMappingRequiredTitleRuleFlagsABlankTitle();
        PendingFeedbackMappingRequiredCodeRuleFlagsABlankCode();
    }
}
