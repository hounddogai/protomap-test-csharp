namespace Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>A rule a benchmark tier must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkTierRule
{
    List<string> Check(BenchmarkTier item);
}

public sealed record BenchmarkTierRequiredOwnerIdRule(string Message) : IBenchmarkTierRule
{
    public List<string> Check(BenchmarkTier item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkTierRequiredTitleRule(string Message) : IBenchmarkTierRule
{
    public List<string> Check(BenchmarkTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkTierRequiredCodeRule(string Message) : IBenchmarkTierRule
{
    public List<string> Check(BenchmarkTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark tier.</summary>
public sealed record BenchmarkTierValidator(List<IBenchmarkTierRule> Rules)
{
    public List<string> Validate(BenchmarkTier item)
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

    public void RequireValid(BenchmarkTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkTier> items)
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

/// <summary>Functions of benchmark tier rules.</summary>
public static class BenchmarkTierRules
{
    public static BenchmarkTierValidator DefaultBenchmarkTierValidator()
    {
        return new BenchmarkTierValidator(
            new List<IBenchmarkTierRule>
            {
                new BenchmarkTierRequiredOwnerIdRule("benchmark tier needs a value"),
                new BenchmarkTierRequiredTitleRule("benchmark tier needs this field"),
                new BenchmarkTierRequiredCodeRule("benchmark tier needs a value"),
            });
    }
}
