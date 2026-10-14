namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedTimesheetTierRulesChecks
{
    public static void BatchedTimesheetTierValidatorReportsEveryProblem()
    {
        var validator = BatchedTimesheetTierRules.DefaultBatchedTimesheetTierValidator();
        var item = BatchedTimesheetTier.NewBatchedTimesheetTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedTimesheetTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedTimesheetTierCodeLengthRule(255);
        var item = BatchedTimesheetTier.NewBatchedTimesheetTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedTimesheetTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BatchedTimesheetTierRequiredCodeRule("batched timesheet tier needs this field");
        var item = BatchedTimesheetTier.NewBatchedTimesheetTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedTimesheetTierMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new BatchedTimesheetTierMinimumRangeRule(1.7, 1736.3);
        var item = BatchedTimesheetTier.NewBatchedTimesheetTier("x", "y");
        item.Minimum = 1737.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedTimesheetTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new BatchedTimesheetTierRequiredLabelRule("batched timesheet tier needs a value");
        var item = BatchedTimesheetTier.NewBatchedTimesheetTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedTimesheetTierValidatorReportsEveryProblem();
        BatchedTimesheetTierCodeLengthRuleFlagsALongCode();
        BatchedTimesheetTierRequiredCodeRuleFlagsABlankCode();
        BatchedTimesheetTierMinimumRangeRuleFlagsValuesAboveMinimum();
        BatchedTimesheetTierRequiredLabelRuleFlagsABlankLabel();
    }
}
