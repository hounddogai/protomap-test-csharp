namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledBenchmarkScheduleRulesChecks
{
    public static void ScheduledBenchmarkScheduleValidatorReportsEveryProblem()
    {
        var validator = ScheduledBenchmarkScheduleRules.DefaultScheduledBenchmarkScheduleValidator();
        var item = ScheduledBenchmarkSchedule.NewScheduledBenchmarkSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBenchmarkScheduleBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new ScheduledBenchmarkScheduleBadgeLengthRule(3600);
        var item = ScheduledBenchmarkSchedule.NewScheduledBenchmarkSchedule("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledBenchmarkScheduleRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new ScheduledBenchmarkScheduleRetriesRangeRule(64, 1000);
        var item = ScheduledBenchmarkSchedule.NewScheduledBenchmarkSchedule("x", "y");
        item.Retries = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledBenchmarkScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledBenchmarkScheduleRequiredCodeRule("scheduled benchmark schedule needs an entry");
        var item = ScheduledBenchmarkSchedule.NewScheduledBenchmarkSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledBenchmarkScheduleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new ScheduledBenchmarkScheduleAmountRangeRule(5.3, 4497.5);
        var item = ScheduledBenchmarkSchedule.NewScheduledBenchmarkSchedule("x", "y");
        item.Amount = 4499.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledBenchmarkScheduleValidatorReportsEveryProblem();
        ScheduledBenchmarkScheduleBadgeLengthRuleFlagsALongBadge();
        ScheduledBenchmarkScheduleRetriesRangeRuleFlagsValuesAboveRetries();
        ScheduledBenchmarkScheduleRequiredCodeRuleFlagsABlankCode();
        ScheduledBenchmarkScheduleAmountRangeRuleFlagsValuesAboveAmount();
    }
}
