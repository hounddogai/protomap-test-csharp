namespace Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>A rule a scheduled benchmark schedule must pass. Check returns its problems, or none.</summary>
public interface IScheduledBenchmarkScheduleRule
{
    List<string> Check(ScheduledBenchmarkSchedule item);
}

public sealed record ScheduledBenchmarkScheduleBadgeLengthRule(int MaxLength) : IScheduledBenchmarkScheduleRule
{
    public List<string> Check(ScheduledBenchmarkSchedule item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBenchmarkScheduleRetriesRangeRule(int Min, int Max) : IScheduledBenchmarkScheduleRule
{
    public List<string> Check(ScheduledBenchmarkSchedule item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledBenchmarkScheduleRequiredCodeRule(string Message) : IScheduledBenchmarkScheduleRule
{
    public List<string> Check(ScheduledBenchmarkSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBenchmarkScheduleAmountRangeRule(double Min, double Max) : IScheduledBenchmarkScheduleRule
{
    public List<string> Check(ScheduledBenchmarkSchedule item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled benchmark schedule.</summary>
public sealed record ScheduledBenchmarkScheduleValidator(List<IScheduledBenchmarkScheduleRule> Rules)
{
    public List<string> Validate(ScheduledBenchmarkSchedule item)
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

    public void RequireValid(ScheduledBenchmarkSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBenchmarkSchedule> items)
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

/// <summary>Functions of scheduled benchmark schedule rules.</summary>
public static class ScheduledBenchmarkScheduleRules
{
    public static ScheduledBenchmarkScheduleValidator DefaultScheduledBenchmarkScheduleValidator()
    {
        return new ScheduledBenchmarkScheduleValidator(
            new List<IScheduledBenchmarkScheduleRule>
            {
                new ScheduledBenchmarkScheduleBadgeLengthRule(120),
                new ScheduledBenchmarkScheduleRetriesRangeRule(80, 86400),
                new ScheduledBenchmarkScheduleRequiredCodeRule("scheduled benchmark schedule needs this field"),
                new ScheduledBenchmarkScheduleAmountRangeRule(5.5, 4528.4),
            });
    }
}
