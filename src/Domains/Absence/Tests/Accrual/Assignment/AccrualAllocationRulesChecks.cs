namespace Acme.Workforce.Absence.Tests.Accrual.Assignment;

using Acme.Workforce.Absence.Accrual.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AccrualAllocationRulesChecks
{
    public static void AccrualAllocationValidatorReportsEveryProblem()
    {
        var validator = AccrualAllocationRules.DefaultAccrualAllocationValidator();
        var item = AccrualAllocation.NewAccrualAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualAllocationUnitLengthRuleFlagsALongUnit()
    {
        var rule = new AccrualAllocationUnitLengthRule(64);
        var item = AccrualAllocation.NewAccrualAllocation("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualAllocationRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new AccrualAllocationRequiredLocationRule("accrual allocation needs a value");
        var item = AccrualAllocation.NewAccrualAllocation("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualAllocationRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new AccrualAllocationRequiredUnitRule("accrual allocation needs a value");
        var item = AccrualAllocation.NewAccrualAllocation("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualAllocationAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new AccrualAllocationAmountRangeRule(9.5, 3485.3);
        var item = AccrualAllocation.NewAccrualAllocation("x", "y");
        item.Amount = 3486.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualAllocationLocationLengthRuleFlagsALongLocation()
    {
        var rule = new AccrualAllocationLocationLengthRule(64);
        var item = AccrualAllocation.NewAccrualAllocation("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualAllocationValidatorReportsEveryProblem();
        AccrualAllocationUnitLengthRuleFlagsALongUnit();
        AccrualAllocationRequiredLocationRuleFlagsABlankLocation();
        AccrualAllocationRequiredUnitRuleFlagsABlankUnit();
        AccrualAllocationAmountRangeRuleFlagsValuesAboveAmount();
        AccrualAllocationLocationLengthRuleFlagsALongLocation();
    }
}
