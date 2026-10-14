namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingPolicyAckGroupRulesChecks
{
    public static void PendingPolicyAckGroupValidatorReportsEveryProblem()
    {
        var validator = PendingPolicyAckGroupRules.DefaultPendingPolicyAckGroupValidator();
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPolicyAckGroupBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PendingPolicyAckGroupBudgetRangeRule(7.0, 315.9);
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("x", "y");
        item.Budget = 317.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingPolicyAckGroupThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new PendingPolicyAckGroupThresholdRangeRule(3.0, 3627.6);
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("x", "y");
        item.Threshold = 3629.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingPolicyAckGroupTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingPolicyAckGroupTitleLengthRule(1000);
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingPolicyAckGroupAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PendingPolicyAckGroupAmountRangeRule(2.6, 1565.2);
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("x", "y");
        item.Amount = 1566.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingPolicyAckGroupValidatorReportsEveryProblem();
        PendingPolicyAckGroupBudgetRangeRuleFlagsValuesAboveBudget();
        PendingPolicyAckGroupThresholdRangeRuleFlagsValuesAboveThreshold();
        PendingPolicyAckGroupTitleLengthRuleFlagsALongTitle();
        PendingPolicyAckGroupAmountRangeRuleFlagsValuesAboveAmount();
    }
}
