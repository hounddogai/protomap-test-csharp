namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark rollup must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkRollupRule
{
    List<string> Check(BenchmarkRollup item);
}

public sealed record BenchmarkRollupRetriesRangeRule(int Min, int Max) : IBenchmarkRollupRule
{
    public List<string> Check(BenchmarkRollup item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkRollupRequiredNameRule(string Message) : IBenchmarkRollupRule
{
    public List<string> Check(BenchmarkRollup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkRollupAllowedSegmentRule(List<string> Allowed) : IBenchmarkRollupRule
{
    public List<string> Check(BenchmarkRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark rollup.</summary>
public sealed record BenchmarkRollupValidator(List<IBenchmarkRollupRule> Rules)
{
    public List<string> Validate(BenchmarkRollup item)
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

    public void RequireValid(BenchmarkRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkRollup> items)
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

/// <summary>Functions of benchmark rollup rules.</summary>
public static class BenchmarkRollupRules
{
    public static BenchmarkRollupValidator DefaultBenchmarkRollupValidator()
    {
        return new BenchmarkRollupValidator(
            new List<IBenchmarkRollupRule>
            {
                new BenchmarkRollupRetriesRangeRule(16, 64),
                new BenchmarkRollupRequiredNameRule("benchmark rollup needs an entry"),
                new BenchmarkRollupAllowedSegmentRule(new List<string> { "scheduled", "default", "adaptive" }),
            });
    }
}
