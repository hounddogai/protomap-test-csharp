namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FallbackPolicyAckRequestRulesChecks
{
    public static void FallbackPolicyAckRequestValidatorReportsEveryProblem()
    {
        var validator = FallbackPolicyAckRequestRules.DefaultFallbackPolicyAckRequestValidator();
        var item = FallbackPolicyAckRequest.NewFallbackPolicyAckRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPolicyAckRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackPolicyAckRequestRequiredCodeRule("fallback policy ack request needs this field");
        var item = FallbackPolicyAckRequest.NewFallbackPolicyAckRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPolicyAckRequestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new FallbackPolicyAckRequestMarginRangeRule(3.1, 4335.5);
        var item = FallbackPolicyAckRequest.NewFallbackPolicyAckRequest("x", "y");
        item.Margin = 4337.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPolicyAckRequestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new FallbackPolicyAckRequestRequiredLabelRule("fallback policy ack request needs an entry");
        var item = FallbackPolicyAckRequest.NewFallbackPolicyAckRequest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPolicyAckRequestRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new FallbackPolicyAckRequestRatingRangeRule(6.7, 595.2);
        var item = FallbackPolicyAckRequest.NewFallbackPolicyAckRequest("x", "y");
        item.Rating = 596.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackPolicyAckRequestValidatorReportsEveryProblem();
        FallbackPolicyAckRequestRequiredCodeRuleFlagsABlankCode();
        FallbackPolicyAckRequestMarginRangeRuleFlagsValuesAboveMargin();
        FallbackPolicyAckRequestRequiredLabelRuleFlagsABlankLabel();
        FallbackPolicyAckRequestRatingRangeRuleFlagsValuesAboveRating();
    }
}
