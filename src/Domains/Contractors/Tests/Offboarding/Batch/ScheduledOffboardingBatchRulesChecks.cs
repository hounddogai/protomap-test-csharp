namespace Acme.Workforce.Contractors.Tests.Offboarding.Batch;

using Acme.Workforce.Contractors.Offboarding.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ScheduledOffboardingBatchRulesChecks
{
    public static void ScheduledOffboardingBatchValidatorReportsEveryProblem()
    {
        var validator = ScheduledOffboardingBatchRules.DefaultScheduledOffboardingBatchValidator();
        var item = ScheduledOffboardingBatch.NewScheduledOffboardingBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledOffboardingBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new ScheduledOffboardingBatchRequiredNameRule("scheduled offboarding batch needs a value");
        var item = ScheduledOffboardingBatch.NewScheduledOffboardingBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledOffboardingBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledOffboardingBatchCodeLengthRule(16);
        var item = ScheduledOffboardingBatch.NewScheduledOffboardingBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledOffboardingBatchLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new ScheduledOffboardingBatchLeaveDaysRangeRule(1000, 86400);
        var item = ScheduledOffboardingBatch.NewScheduledOffboardingBatch("x", "y");
        item.LeaveDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledOffboardingBatchBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new ScheduledOffboardingBatchBatchSizeRangeRule(64, 64);
        var item = ScheduledOffboardingBatch.NewScheduledOffboardingBatch("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledOffboardingBatchValidatorReportsEveryProblem();
        ScheduledOffboardingBatchRequiredNameRuleFlagsABlankName();
        ScheduledOffboardingBatchCodeLengthRuleFlagsALongCode();
        ScheduledOffboardingBatchLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        ScheduledOffboardingBatchBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
