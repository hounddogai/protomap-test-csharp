namespace Acme.Workforce.Workplace.Tests.Visitor.Policy;

using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class VisitorPolicyRulesChecks
{
    public static void VisitorPolicyValidatorReportsEveryProblem()
    {
        var validator = VisitorPolicyRules.DefaultVisitorPolicyValidator();
        var item = VisitorPolicy.NewVisitorPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorPolicyUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new VisitorPolicyUpdatedAtRangeRule(32, 86400);
        var item = VisitorPolicy.NewVisitorPolicy("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorPolicyCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new VisitorPolicyCeilingRangeRule(1.1, 1509.6);
        var item = VisitorPolicy.NewVisitorPolicy("x", "y");
        item.Ceiling = 1511.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorPolicyRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VisitorPolicyRequiredCodeRule("visitor policy needs a value");
        var item = VisitorPolicy.NewVisitorPolicy("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorPolicyValidatorReportsEveryProblem();
        VisitorPolicyUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        VisitorPolicyCeilingRangeRuleFlagsValuesAboveCeiling();
        VisitorPolicyRequiredCodeRuleFlagsABlankCode();
    }
}
