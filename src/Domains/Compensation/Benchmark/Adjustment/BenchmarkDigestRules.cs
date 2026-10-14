namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>A rule a benchmark digest must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkDigestRule
{
    List<string> Check(BenchmarkDigest item);
}

public sealed record BenchmarkDigestRequiredTimezoneRule(string Message) : IBenchmarkDigestRule
{
    public List<string> Check(BenchmarkDigest item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkDigestCreatedAtRangeRule(int Min, int Max) : IBenchmarkDigestRule
{
    public List<string> Check(BenchmarkDigest item)
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

public sealed record BenchmarkDigestRequiredCodeRule(string Message) : IBenchmarkDigestRule
{
    public List<string> Check(BenchmarkDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkDigestRequiredTitleRule(string Message) : IBenchmarkDigestRule
{
    public List<string> Check(BenchmarkDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark digest.</summary>
public sealed record BenchmarkDigestValidator(List<IBenchmarkDigestRule> Rules)
{
    public List<string> Validate(BenchmarkDigest item)
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

    public void RequireValid(BenchmarkDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkDigest> items)
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

/// <summary>Functions of benchmark digest rules.</summary>
public static class BenchmarkDigestRules
{
    public static BenchmarkDigestValidator DefaultBenchmarkDigestValidator()
    {
        return new BenchmarkDigestValidator(
            new List<IBenchmarkDigestRule>
            {
                new BenchmarkDigestRequiredTimezoneRule("benchmark digest needs an entry"),
                new BenchmarkDigestCreatedAtRangeRule(32, 255),
                new BenchmarkDigestRequiredCodeRule("benchmark digest needs this field"),
                new BenchmarkDigestRequiredTitleRule("benchmark digest needs this field"),
            });
    }
}
