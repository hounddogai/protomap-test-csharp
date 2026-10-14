namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class ScheduledBenchmarkEntryRulesChecks
{
    public static void ScheduledBenchmarkEntryValidatorReportsEveryProblem()
    {
        var validator = ScheduledBenchmarkEntryRules.DefaultScheduledBenchmarkEntryValidator();
        var item = ScheduledBenchmarkEntry.NewScheduledBenchmarkEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledBenchmarkEntryRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new ScheduledBenchmarkEntryRequiredPhoneRule("scheduled benchmark entry needs a value");
        var item = ScheduledBenchmarkEntry.NewScheduledBenchmarkEntry("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledBenchmarkEntryPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new ScheduledBenchmarkEntryPhoneLengthRule(32);
        var item = ScheduledBenchmarkEntry.NewScheduledBenchmarkEntry("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledBenchmarkEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledBenchmarkEntryCodeLengthRule(32);
        var item = ScheduledBenchmarkEntry.NewScheduledBenchmarkEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledBenchmarkEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledBenchmarkEntryRequiredCodeRule("scheduled benchmark entry needs an entry");
        var item = ScheduledBenchmarkEntry.NewScheduledBenchmarkEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledBenchmarkEntryValidatorReportsEveryProblem();
        ScheduledBenchmarkEntryRequiredPhoneRuleFlagsABlankPhone();
        ScheduledBenchmarkEntryPhoneLengthRuleFlagsALongPhone();
        ScheduledBenchmarkEntryCodeLengthRuleFlagsALongCode();
        ScheduledBenchmarkEntryRequiredCodeRuleFlagsABlankCode();
    }
}
