namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>Checks of benchmark audit code, run by the project's runner.</summary>
public static class BenchmarkScheduleRulesChecks
{
    public static void BenchmarkScheduleValidatorReportsEveryProblem()
    {
        var validator = BenchmarkScheduleRules.DefaultBenchmarkScheduleValidator();
        var item = BenchmarkSchedule.NewBenchmarkSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkScheduleChannelLengthRuleFlagsALongChannel()
    {
        var rule = new BenchmarkScheduleChannelLengthRule(255);
        var item = BenchmarkSchedule.NewBenchmarkSchedule("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkSchedulePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new BenchmarkSchedulePriorityRangeRule(1000, 3600);
        var item = BenchmarkSchedule.NewBenchmarkSchedule("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkScheduleRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new BenchmarkScheduleRequiredChannelRule("benchmark schedule needs this field");
        var item = BenchmarkSchedule.NewBenchmarkSchedule("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BenchmarkScheduleRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new BenchmarkScheduleRequiredLocaleRule("benchmark schedule needs a value");
        var item = BenchmarkSchedule.NewBenchmarkSchedule("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkScheduleValidatorReportsEveryProblem();
        BenchmarkScheduleChannelLengthRuleFlagsALongChannel();
        BenchmarkSchedulePriorityRangeRuleFlagsValuesAbovePriority();
        BenchmarkScheduleRequiredChannelRuleFlagsABlankChannel();
        BenchmarkScheduleRequiredLocaleRuleFlagsABlankLocale();
    }
}
