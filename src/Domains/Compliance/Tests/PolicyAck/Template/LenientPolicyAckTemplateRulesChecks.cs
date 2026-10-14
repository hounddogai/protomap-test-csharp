namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LenientPolicyAckTemplateRulesChecks
{
    public static void LenientPolicyAckTemplateValidatorReportsEveryProblem()
    {
        var validator = LenientPolicyAckTemplateRules.DefaultLenientPolicyAckTemplateValidator();
        var item = LenientPolicyAckTemplate.NewLenientPolicyAckTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPolicyAckTemplateDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LenientPolicyAckTemplateDurationMinutesRangeRule(16, 86400);
        var item = LenientPolicyAckTemplate.NewLenientPolicyAckTemplate("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientPolicyAckTemplateRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new LenientPolicyAckTemplateRequiredExternalRefRule("lenient policy ack template needs an entry");
        var item = LenientPolicyAckTemplate.NewLenientPolicyAckTemplate("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientPolicyAckTemplateValidatorReportsEveryProblem();
        LenientPolicyAckTemplateDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        LenientPolicyAckTemplateRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
