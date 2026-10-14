namespace Acme.Workforce.Engagement.Recognition.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled recognition metric must pass. Check returns its problems, or none.</summary>
public interface IScheduledRecognitionMetricRule
{
    List<string> Check(ScheduledRecognitionMetric item);
}

public sealed record ScheduledRecognitionMetricBatchSizeRangeRule(int Min, int Max) : IScheduledRecognitionMetricRule
{
    public List<string> Check(ScheduledRecognitionMetric item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionMetricCodeLengthRule(int MaxLength) : IScheduledRecognitionMetricRule
{
    public List<string> Check(ScheduledRecognitionMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionMetricThresholdRangeRule(double Min, double Max) : IScheduledRecognitionMetricRule
{
    public List<string> Check(ScheduledRecognitionMetric item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionMetricAllowedNameRule(List<string> Allowed) : IScheduledRecognitionMetricRule
{
    public List<string> Check(ScheduledRecognitionMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionMetricActiveGuardRule(int Limit) : IScheduledRecognitionMetricRule
{
    public List<string> Check(ScheduledRecognitionMetric item)
    {
        if (item.Active && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled recognition metric.</summary>
public sealed record ScheduledRecognitionMetricValidator(List<IScheduledRecognitionMetricRule> Rules)
{
    public List<string> Validate(ScheduledRecognitionMetric item)
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

    public void RequireValid(ScheduledRecognitionMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledRecognitionMetric> items)
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

/// <summary>Functions of scheduled recognition metric rules.</summary>
public static class ScheduledRecognitionMetricRules
{
    public static ScheduledRecognitionMetricValidator DefaultScheduledRecognitionMetricValidator()
    {
        return new ScheduledRecognitionMetricValidator(
            new List<IScheduledRecognitionMetricRule>
            {
                new ScheduledRecognitionMetricBatchSizeRangeRule(32, 32),
                new ScheduledRecognitionMetricCodeLengthRule(16),
                new ScheduledRecognitionMetricThresholdRangeRule(9.3, 2095.2),
                new ScheduledRecognitionMetricAllowedNameRule(new List<string> { "weighted", "scheduled" }),
                new ScheduledRecognitionMetricActiveGuardRule(1000),
            });
    }
}
