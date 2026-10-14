namespace Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>A rule a benchmark estimate must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkEstimateRule
{
    List<string> Check(BenchmarkEstimate item);
}

public sealed record BenchmarkEstimateRequiredCodeRule(string Message) : IBenchmarkEstimateRule
{
    public List<string> Check(BenchmarkEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkEstimateRequiredPhoneRule(string Message) : IBenchmarkEstimateRule
{
    public List<string> Check(BenchmarkEstimate item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkEstimateMarginRangeRule(double Min, double Max) : IBenchmarkEstimateRule
{
    public List<string> Check(BenchmarkEstimate item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark estimate.</summary>
public sealed record BenchmarkEstimateValidator(List<IBenchmarkEstimateRule> Rules)
{
    public List<string> Validate(BenchmarkEstimate item)
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

    public void RequireValid(BenchmarkEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkEstimate> items)
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

/// <summary>Functions of benchmark estimate rules.</summary>
public static class BenchmarkEstimateRules
{
    public static BenchmarkEstimateValidator DefaultBenchmarkEstimateValidator()
    {
        return new BenchmarkEstimateValidator(
            new List<IBenchmarkEstimateRule>
            {
                new BenchmarkEstimateRequiredCodeRule("benchmark estimate needs an entry"),
                new BenchmarkEstimateRequiredPhoneRule("benchmark estimate needs an entry"),
                new BenchmarkEstimateMarginRangeRule(4.4, 1013.7),
            });
    }
}
