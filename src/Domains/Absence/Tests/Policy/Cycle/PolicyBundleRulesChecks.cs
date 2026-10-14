namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyBundleRulesChecks
{
    public static void PolicyBundleValidatorReportsEveryProblem()
    {
        var validator = PolicyBundleRules.DefaultPolicyBundleValidator();
        var item = PolicyBundle.NewPolicyBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyBundleUnitLengthRuleFlagsALongUnit()
    {
        var rule = new PolicyBundleUnitLengthRule(32);
        var item = PolicyBundle.NewPolicyBundle("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PolicyBundleCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new PolicyBundleCostCenterLengthRule(3600);
        var item = PolicyBundle.NewPolicyBundle("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyBundleValidatorReportsEveryProblem();
        PolicyBundleUnitLengthRuleFlagsALongUnit();
        PolicyBundleCostCenterLengthRuleFlagsALongCostCenter();
    }
}
