namespace Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>A rule a benchmark window must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkWindowRule
{
    List<string> Check(BenchmarkWindow item);
}

public sealed record BenchmarkWindowVersionRangeRule(int Min, int Max) : IBenchmarkWindowRule
{
    public List<string> Check(BenchmarkWindow item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkWindowRateRangeRule(double Min, double Max) : IBenchmarkWindowRule
{
    public List<string> Check(BenchmarkWindow item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkWindowRequiredTitleRule(string Message) : IBenchmarkWindowRule
{
    public List<string> Check(BenchmarkWindow item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark window.</summary>
public sealed record BenchmarkWindowValidator(List<IBenchmarkWindowRule> Rules)
{
    public List<string> Validate(BenchmarkWindow item)
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

    public void RequireValid(BenchmarkWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkWindow> items)
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

/// <summary>Functions of benchmark window rules.</summary>
public static class BenchmarkWindowRules
{
    public static BenchmarkWindowValidator DefaultBenchmarkWindowValidator()
    {
        return new BenchmarkWindowValidator(
            new List<IBenchmarkWindowRule>
            {
                new BenchmarkWindowVersionRangeRule(80, 86400),
                new BenchmarkWindowRateRangeRule(7.5, 4514.4),
                new BenchmarkWindowRequiredTitleRule("benchmark window needs a value"),
            });
    }
}
