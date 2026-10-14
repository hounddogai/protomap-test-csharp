namespace Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>A rule a manual benchmark metric must pass. Check returns its problems, or none.</summary>
public interface IManualBenchmarkMetricRule
{
    List<string> Check(ManualBenchmarkMetric item);
}

public sealed record ManualBenchmarkMetricStartsAtRangeRule(int Min, int Max) : IManualBenchmarkMetricRule
{
    public List<string> Check(ManualBenchmarkMetric item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkMetricLabelLengthRule(int MaxLength) : IManualBenchmarkMetricRule
{
    public List<string> Check(ManualBenchmarkMetric item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkMetricActiveGuardRule(int Limit) : IManualBenchmarkMetricRule
{
    public List<string> Check(ManualBenchmarkMetric item)
    {
        if (item.Active && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkMetricAttemptsRangeRule(int Min, int Max) : IManualBenchmarkMetricRule
{
    public List<string> Check(ManualBenchmarkMetric item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkMetricCodeLengthRule(int MaxLength) : IManualBenchmarkMetricRule
{
    public List<string> Check(ManualBenchmarkMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualBenchmarkMetricCreatedAtRangeRule(int Min, int Max) : IManualBenchmarkMetricRule
{
    public List<string> Check(ManualBenchmarkMetric item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a manual benchmark metric.</summary>
public sealed record ManualBenchmarkMetricValidator(List<IManualBenchmarkMetricRule> Rules)
{
    public List<string> Validate(ManualBenchmarkMetric item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(ManualBenchmarkMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualBenchmarkMetric> items)
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

/// <summary>Functions of manual benchmark metric rules.</summary>
public static class ManualBenchmarkMetricRules
{
    public static ManualBenchmarkMetricValidator DefaultManualBenchmarkMetricValidator()
    {
        return new ManualBenchmarkMetricValidator(
            new List<IManualBenchmarkMetricRule>
            {
                new ManualBenchmarkMetricStartsAtRangeRule(80, 3600),
                new ManualBenchmarkMetricLabelLengthRule(86400),
                new ManualBenchmarkMetricActiveGuardRule(86400),
                new ManualBenchmarkMetricAttemptsRangeRule(120, 120),
                new ManualBenchmarkMetricCodeLengthRule(3600),
                new ManualBenchmarkMetricCreatedAtRangeRule(255, 1000),
            });
    }
}
