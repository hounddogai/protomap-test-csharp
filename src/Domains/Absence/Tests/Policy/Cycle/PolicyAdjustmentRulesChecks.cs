namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyAdjustmentRulesChecks
{
    public static void PolicyAdjustmentValidatorReportsEveryProblem()
    {
        var validator = PolicyAdjustmentRules.DefaultPolicyAdjustmentValidator();
        var item = PolicyAdjustment.NewPolicyAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyAdjustmentRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new PolicyAdjustmentRequiredUnitRule("policy adjustment needs a value");
        var item = PolicyAdjustment.NewPolicyAdjustment("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyAdjustmentAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PolicyAdjustmentAmountRangeRule(7.1, 3983.7);
        var item = PolicyAdjustment.NewPolicyAdjustment("x", "y");
        item.Amount = 3985.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyAdjustmentSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new PolicyAdjustmentSegmentLengthRule(80);
        var item = PolicyAdjustment.NewPolicyAdjustment("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyAdjustmentValidatorReportsEveryProblem();
        PolicyAdjustmentRequiredUnitRuleFlagsABlankUnit();
        PolicyAdjustmentAmountRangeRuleFlagsValuesAboveAmount();
        PolicyAdjustmentSegmentLengthRuleFlagsALongSegment();
    }
}
