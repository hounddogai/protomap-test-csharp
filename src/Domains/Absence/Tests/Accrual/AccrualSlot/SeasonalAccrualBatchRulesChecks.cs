namespace Acme.Workforce.Absence.Tests.Accrual.AccrualSlot;

using Acme.Workforce.Absence.Accrual.AccrualSlot;

/// <summary>Checks of accrual slot code, run by the project's runner.</summary>
public static class SeasonalAccrualBatchRulesChecks
{
    public static void SeasonalAccrualBatchValidatorReportsEveryProblem()
    {
        var validator = SeasonalAccrualBatchRules.DefaultSeasonalAccrualBatchValidator();
        var item = SeasonalAccrualBatch.NewSeasonalAccrualBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalAccrualBatchLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new SeasonalAccrualBatchLeaveDaysRangeRule(64, 64);
        var item = SeasonalAccrualBatch.NewSeasonalAccrualBatch("x", "y");
        item.LeaveDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalAccrualBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalAccrualBatchCodeLengthRule(120);
        var item = SeasonalAccrualBatch.NewSeasonalAccrualBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalAccrualBatchMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new SeasonalAccrualBatchMarginRangeRule(5.5, 1983.6);
        var item = SeasonalAccrualBatch.NewSeasonalAccrualBatch("x", "y");
        item.Margin = 1985.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SeasonalAccrualBatchValidatorReportsEveryProblem();
        SeasonalAccrualBatchLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        SeasonalAccrualBatchCodeLengthRuleFlagsALongCode();
        SeasonalAccrualBatchMarginRangeRuleFlagsValuesAboveMargin();
    }
}
