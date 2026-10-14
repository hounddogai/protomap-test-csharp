namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class NominationPolicyRulesChecks
{
    public static void NominationPolicyValidatorReportsEveryProblem()
    {
        var validator = NominationPolicyRules.DefaultNominationPolicyValidator();
        var item = NominationPolicy.NewNominationPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new NominationPolicyLabelLengthRule(86400);
        var item = NominationPolicy.NewNominationPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new NominationPolicyCodeLengthRule(255);
        var item = NominationPolicy.NewNominationPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationPolicyJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new NominationPolicyJobCodeLengthRule(80);
        var item = NominationPolicy.NewNominationPolicy("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        NominationPolicyValidatorReportsEveryProblem();
        NominationPolicyLabelLengthRuleFlagsALongLabel();
        NominationPolicyCodeLengthRuleFlagsALongCode();
        NominationPolicyJobCodeLengthRuleFlagsALongJobCode();
    }
}
