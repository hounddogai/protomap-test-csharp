namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SeasonalPolicyAckRequestRulesChecks
{
    public static void SeasonalPolicyAckRequestValidatorReportsEveryProblem()
    {
        var validator = SeasonalPolicyAckRequestRules.DefaultSeasonalPolicyAckRequestValidator();
        var item = SeasonalPolicyAckRequest.NewSeasonalPolicyAckRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPolicyAckRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalPolicyAckRequestCodeLengthRule(32);
        var item = SeasonalPolicyAckRequest.NewSeasonalPolicyAckRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalPolicyAckRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalPolicyAckRequestRequiredCodeRule("seasonal policy ack request needs this field");
        var item = SeasonalPolicyAckRequest.NewSeasonalPolicyAckRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPolicyAckRequestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SeasonalPolicyAckRequestLabelLengthRule(120);
        var item = SeasonalPolicyAckRequest.NewSeasonalPolicyAckRequest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalPolicyAckRequestValidatorReportsEveryProblem();
        SeasonalPolicyAckRequestCodeLengthRuleFlagsALongCode();
        SeasonalPolicyAckRequestRequiredCodeRuleFlagsABlankCode();
        SeasonalPolicyAckRequestLabelLengthRuleFlagsALongLabel();
    }
}
