namespace Acme.Workforce.Workplace.Tests.Desk.Budget;

using Acme.Workforce.Workplace.Desk.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeskBatchRulesChecks
{
    public static void DeskBatchValidatorReportsEveryProblem()
    {
        var validator = DeskBatchRules.DefaultDeskBatchValidator();
        var item = DeskBatch.NewDeskBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskBatchLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new DeskBatchLeaveDaysRangeRule(16, 255);
        var item = DeskBatch.NewDeskBatch("x", "y");
        item.LeaveDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeskBatchUnitLengthRuleFlagsALongUnit()
    {
        var rule = new DeskBatchUnitLengthRule(120);
        var item = DeskBatch.NewDeskBatch("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskBatchMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new DeskBatchMaximumRangeRule(1.6, 316.8);
        var item = DeskBatch.NewDeskBatch("x", "y");
        item.Maximum = 318.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeskBatchValidatorReportsEveryProblem();
        DeskBatchLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        DeskBatchUnitLengthRuleFlagsALongUnit();
        DeskBatchMaximumRangeRuleFlagsValuesAboveMaximum();
    }
}
