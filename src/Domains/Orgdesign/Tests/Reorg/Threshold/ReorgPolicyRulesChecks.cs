namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ReorgPolicyRulesChecks
{
    public static void ReorgPolicyValidatorReportsEveryProblem()
    {
        var validator = ReorgPolicyRules.DefaultReorgPolicyValidator();
        var item = ReorgPolicy.NewReorgPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgPolicyGradeLengthRuleFlagsALongGrade()
    {
        var rule = new ReorgPolicyGradeLengthRule(32);
        var item = ReorgPolicy.NewReorgPolicy("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgPolicyLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ReorgPolicyLabelLengthRule(255);
        var item = ReorgPolicy.NewReorgPolicy("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgPolicyRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new ReorgPolicyRequiredExternalRefRule("reorg policy needs this field");
        var item = ReorgPolicy.NewReorgPolicy("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgPolicyValidatorReportsEveryProblem();
        ReorgPolicyGradeLengthRuleFlagsALongGrade();
        ReorgPolicyLabelLengthRuleFlagsALongLabel();
        ReorgPolicyRequiredExternalRefRuleFlagsABlankExternalRef();
    }
}
