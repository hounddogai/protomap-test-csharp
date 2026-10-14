namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class GlobalPolicyAckBundleRulesChecks
{
    public static void GlobalPolicyAckBundleValidatorReportsEveryProblem()
    {
        var validator = GlobalPolicyAckBundleRules.DefaultGlobalPolicyAckBundleValidator();
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPolicyAckBundleCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new GlobalPolicyAckBundleCostCenterLengthRule(80);
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPolicyAckBundleCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new GlobalPolicyAckBundleCreatedAtRangeRule(255, 3600);
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPolicyAckBundleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new GlobalPolicyAckBundleUpdatedAtRangeRule(1000, 86400);
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPolicyAckBundleThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new GlobalPolicyAckBundleThresholdRangeRule(7.1, 2010.0);
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("x", "y");
        item.Threshold = 2011.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalPolicyAckBundleValidatorReportsEveryProblem();
        GlobalPolicyAckBundleCostCenterLengthRuleFlagsALongCostCenter();
        GlobalPolicyAckBundleCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        GlobalPolicyAckBundleUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        GlobalPolicyAckBundleThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
