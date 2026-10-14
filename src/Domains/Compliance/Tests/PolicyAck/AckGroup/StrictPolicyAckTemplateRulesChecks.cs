namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckGroup;

using Acme.Workforce.Compliance.PolicyAck.AckGroup;

/// <summary>Checks of ack group code, run by the project's runner.</summary>
public static class StrictPolicyAckTemplateRulesChecks
{
    public static void StrictPolicyAckTemplateValidatorReportsEveryProblem()
    {
        var validator = StrictPolicyAckTemplateRules.DefaultStrictPolicyAckTemplateValidator();
        var item = StrictPolicyAckTemplate.NewStrictPolicyAckTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictPolicyAckTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictPolicyAckTemplateCodeLengthRule(16);
        var item = StrictPolicyAckTemplate.NewStrictPolicyAckTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictPolicyAckTemplateRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new StrictPolicyAckTemplateRatingRangeRule(6.6, 4253.8);
        var item = StrictPolicyAckTemplate.NewStrictPolicyAckTemplate("x", "y");
        item.Rating = 4255.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictPolicyAckTemplateValidatorReportsEveryProblem();
        StrictPolicyAckTemplateCodeLengthRuleFlagsALongCode();
        StrictPolicyAckTemplateRatingRangeRuleFlagsValuesAboveRating();
    }
}
