namespace Acme.Workforce.Absence.Tests.Balance.Segment;

using Acme.Workforce.Absence.Balance.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BalanceSnapshotRulesChecks
{
    public static void BalanceSnapshotValidatorReportsEveryProblem()
    {
        var validator = BalanceSnapshotRules.DefaultBalanceSnapshotValidator();
        var item = BalanceSnapshot.NewBalanceSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceSnapshotCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new BalanceSnapshotCapacityRangeRule(255, 1000);
        var item = BalanceSnapshot.NewBalanceSnapshot("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceSnapshotAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new BalanceSnapshotAmountRangeRule(2.9, 1889.9);
        var item = BalanceSnapshot.NewBalanceSnapshot("x", "y");
        item.Amount = 1891.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceSnapshotRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new BalanceSnapshotRequiredEmployeeIdRule("balance snapshot needs this field");
        var item = BalanceSnapshot.NewBalanceSnapshot("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BalanceSnapshotValidatorReportsEveryProblem();
        BalanceSnapshotCapacityRangeRuleFlagsValuesAboveCapacity();
        BalanceSnapshotAmountRangeRuleFlagsValuesAboveAmount();
        BalanceSnapshotRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
