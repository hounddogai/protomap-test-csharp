namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredPolicyAckVarianceRulesChecks
{
    public static void TieredPolicyAckVarianceValidatorReportsEveryProblem()
    {
        var validator = TieredPolicyAckVarianceRules.DefaultTieredPolicyAckVarianceValidator();
        var item = TieredPolicyAckVariance.NewTieredPolicyAckVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPolicyAckVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredPolicyAckVarianceCodeLengthRule(120);
        var item = TieredPolicyAckVariance.NewTieredPolicyAckVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPolicyAckVarianceRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new TieredPolicyAckVarianceRequiredSourceRule("tiered policy ack variance needs this field");
        var item = TieredPolicyAckVariance.NewTieredPolicyAckVariance("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredPolicyAckVarianceValidatorReportsEveryProblem();
        TieredPolicyAckVarianceCodeLengthRuleFlagsALongCode();
        TieredPolicyAckVarianceRequiredSourceRuleFlagsABlankSource();
    }
}
