namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>A rule a benchmark metric must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkMetricRule
{
    List<string> Check(BenchmarkMetric item);
}

public sealed record BenchmarkMetricRequiredLocationRule(string Message) : IBenchmarkMetricRule
{
    public List<string> Check(BenchmarkMetric item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkMetricRatingRangeRule(double Min, double Max) : IBenchmarkMetricRule
{
    public List<string> Check(BenchmarkMetric item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkMetricTitleLengthRule(int MaxLength) : IBenchmarkMetricRule
{
    public List<string> Check(BenchmarkMetric item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark metric.</summary>
public sealed record BenchmarkMetricValidator(List<IBenchmarkMetricRule> Rules)
{
    public List<string> Validate(BenchmarkMetric item)
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

    public void RequireValid(BenchmarkMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkMetric> items)
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

/// <summary>Functions of benchmark metric rules.</summary>
public static class BenchmarkMetricRules
{
    public static BenchmarkMetricValidator DefaultBenchmarkMetricValidator()
    {
        return new BenchmarkMetricValidator(
            new List<IBenchmarkMetricRule>
            {
                new BenchmarkMetricRequiredLocationRule("benchmark metric needs an entry"),
                new BenchmarkMetricRatingRangeRule(6.2, 276.9),
                new BenchmarkMetricTitleLengthRule(255),
            });
    }
}
