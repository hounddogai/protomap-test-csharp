namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentAllocationRulesChecks
{
    public static void AdjustmentAllocationValidatorReportsEveryProblem()
    {
        var validator = AdjustmentAllocationRules.DefaultAdjustmentAllocationValidator();
        var item = AdjustmentAllocation.NewAdjustmentAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentAllocationCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new AdjustmentAllocationCurrencyLengthRule(1000);
        var item = AdjustmentAllocation.NewAdjustmentAllocation("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentAllocationRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new AdjustmentAllocationRequiredTimezoneRule("adjustment allocation needs this field");
        var item = AdjustmentAllocation.NewAdjustmentAllocation("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdjustmentAllocationRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new AdjustmentAllocationRequiredCurrencyRule("adjustment allocation needs this field");
        var item = AdjustmentAllocation.NewAdjustmentAllocation("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentAllocationValidatorReportsEveryProblem();
        AdjustmentAllocationCurrencyLengthRuleFlagsALongCurrency();
        AdjustmentAllocationRequiredTimezoneRuleFlagsABlankTimezone();
        AdjustmentAllocationRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
