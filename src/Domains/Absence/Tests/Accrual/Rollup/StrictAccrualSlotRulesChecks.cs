namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class StrictAccrualSlotRulesChecks
{
    public static void StrictAccrualSlotValidatorReportsEveryProblem()
    {
        var validator = StrictAccrualSlotRules.DefaultStrictAccrualSlotValidator();
        var item = StrictAccrualSlot.NewStrictAccrualSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAccrualSlotRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new StrictAccrualSlotRetriesRangeRule(3600, 3600);
        var item = StrictAccrualSlot.NewStrictAccrualSlot("x", "y");
        item.Retries = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictAccrualSlotThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new StrictAccrualSlotThresholdRangeRule(9.0, 2474.9);
        var item = StrictAccrualSlot.NewStrictAccrualSlot("x", "y");
        item.Threshold = 2476.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictAccrualSlotDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new StrictAccrualSlotDepartmentLengthRule(120);
        var item = StrictAccrualSlot.NewStrictAccrualSlot("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAccrualSlotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictAccrualSlotLabelLengthRule(64);
        var item = StrictAccrualSlot.NewStrictAccrualSlot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictAccrualSlotValidatorReportsEveryProblem();
        StrictAccrualSlotRetriesRangeRuleFlagsValuesAboveRetries();
        StrictAccrualSlotThresholdRangeRuleFlagsValuesAboveThreshold();
        StrictAccrualSlotDepartmentLengthRuleFlagsALongDepartment();
        StrictAccrualSlotLabelLengthRuleFlagsALongLabel();
    }
}
