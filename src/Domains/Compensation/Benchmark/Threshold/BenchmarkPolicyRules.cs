namespace Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>A rule a benchmark policy must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkPolicyRule
{
    List<string> Check(BenchmarkPolicy item);
}

public sealed record BenchmarkPolicyRetriesRangeRule(int Min, int Max) : IBenchmarkPolicyRule
{
    public List<string> Check(BenchmarkPolicy item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkPolicyRequiredCodeRule(string Message) : IBenchmarkPolicyRule
{
    public List<string> Check(BenchmarkPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkPolicyBatchSizeRangeRule(int Min, int Max) : IBenchmarkPolicyRule
{
    public List<string> Check(BenchmarkPolicy item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a benchmark policy.</summary>
public sealed record BenchmarkPolicyValidator(List<IBenchmarkPolicyRule> Rules)
{
    public List<string> Validate(BenchmarkPolicy item)
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

    public void RequireValid(BenchmarkPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkPolicy> items)
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

/// <summary>Functions of benchmark policy rules.</summary>
public static class BenchmarkPolicyRules
{
    public static BenchmarkPolicyValidator DefaultBenchmarkPolicyValidator()
    {
        return new BenchmarkPolicyValidator(
            new List<IBenchmarkPolicyRule>
            {
                new BenchmarkPolicyRetriesRangeRule(16, 64),
                new BenchmarkPolicyRequiredCodeRule("benchmark policy needs a value"),
                new BenchmarkPolicyBatchSizeRangeRule(3600, 86400),
            });
    }
}
