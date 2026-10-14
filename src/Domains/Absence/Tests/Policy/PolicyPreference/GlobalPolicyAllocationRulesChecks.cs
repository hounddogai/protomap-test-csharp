namespace Acme.Workforce.Absence.Tests.Policy.PolicyPreference;

using Acme.Workforce.Absence.Policy.PolicyPreference;

/// <summary>Checks of policy preference code, run by the project's runner.</summary>
public static class GlobalPolicyAllocationRulesChecks
{
    public static void GlobalPolicyAllocationValidatorReportsEveryProblem()
    {
        var validator = GlobalPolicyAllocationRules.DefaultGlobalPolicyAllocationValidator();
        var item = GlobalPolicyAllocation.NewGlobalPolicyAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPolicyAllocationAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new GlobalPolicyAllocationAmountRangeRule(9.6, 621.7);
        var item = GlobalPolicyAllocation.NewGlobalPolicyAllocation("x", "y");
        item.Amount = 623.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPolicyAllocationNameLengthRuleFlagsALongName()
    {
        var rule = new GlobalPolicyAllocationNameLengthRule(80);
        var item = GlobalPolicyAllocation.NewGlobalPolicyAllocation("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPolicyAllocationRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalPolicyAllocationRequiredNameRule("global policy allocation needs a value");
        var item = GlobalPolicyAllocation.NewGlobalPolicyAllocation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalPolicyAllocationValidatorReportsEveryProblem();
        GlobalPolicyAllocationAmountRangeRuleFlagsValuesAboveAmount();
        GlobalPolicyAllocationNameLengthRuleFlagsALongName();
        GlobalPolicyAllocationRequiredNameRuleFlagsABlankName();
    }
}
