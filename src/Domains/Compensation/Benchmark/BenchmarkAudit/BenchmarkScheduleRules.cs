namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>A rule a benchmark schedule must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkScheduleRule
{
    List<string> Check(BenchmarkSchedule item);
}

public sealed record BenchmarkScheduleChannelLengthRule(int MaxLength) : IBenchmarkScheduleRule
{
    public List<string> Check(BenchmarkSchedule item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkSchedulePriorityRangeRule(int Min, int Max) : IBenchmarkScheduleRule
{
    public List<string> Check(BenchmarkSchedule item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkScheduleRequiredChannelRule(string Message) : IBenchmarkScheduleRule
{
    public List<string> Check(BenchmarkSchedule item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkScheduleRequiredLocaleRule(string Message) : IBenchmarkScheduleRule
{
    public List<string> Check(BenchmarkSchedule item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark schedule.</summary>
public sealed record BenchmarkScheduleValidator(List<IBenchmarkScheduleRule> Rules)
{
    public List<string> Validate(BenchmarkSchedule item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(BenchmarkSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkSchedule> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of benchmark schedule rules.</summary>
public static class BenchmarkScheduleRules
{
    public static BenchmarkScheduleValidator DefaultBenchmarkScheduleValidator()
    {
        return new BenchmarkScheduleValidator(
            new List<IBenchmarkScheduleRule>
            {
                new BenchmarkScheduleChannelLengthRule(86400),
                new BenchmarkSchedulePriorityRangeRule(255, 3600),
                new BenchmarkScheduleRequiredChannelRule("benchmark schedule needs a value"),
                new BenchmarkScheduleRequiredLocaleRule("benchmark schedule needs this field"),
            });
    }
}
