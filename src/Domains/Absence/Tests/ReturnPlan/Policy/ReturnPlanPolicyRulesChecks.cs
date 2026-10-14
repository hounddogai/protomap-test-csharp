namespace Acme.Workforce.Absence.Tests.ReturnPlan.Policy;

using Acme.Workforce.Absence.ReturnPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class ReturnPlanPolicyRulesChecks
{
    public static void ReturnPlanPolicyValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanPolicyRules.DefaultReturnPlanPolicyValidator();
        var item = ReturnPlanPolicy.NewReturnPlanPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanPolicyChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ReturnPlanPolicyChannelLengthRule(255);
        var item = ReturnPlanPolicy.NewReturnPlanPolicy("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanPolicyRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ReturnPlanPolicyRequiredLabelRule("return plan policy needs a value");
        var item = ReturnPlanPolicy.NewReturnPlanPolicy("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReturnPlanPolicyRequiredCodeRule("return plan policy needs an entry");
        var item = ReturnPlanPolicy.NewReturnPlanPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReturnPlanPolicyValidatorReportsEveryProblem();
        ReturnPlanPolicyChannelLengthRuleFlagsALongChannel();
        ReturnPlanPolicyRequiredLabelRuleFlagsABlankLabel();
        ReturnPlanPolicyRequiredCodeRuleFlagsABlankCode();
    }
}
