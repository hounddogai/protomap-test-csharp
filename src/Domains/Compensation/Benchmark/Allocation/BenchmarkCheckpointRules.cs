namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark checkpoint must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkCheckpointRule
{
    List<string> Check(BenchmarkCheckpoint item);
}

public sealed record BenchmarkCheckpointCodePrefixRule(string Prefix) : IBenchmarkCheckpointRule
{
    public List<string> Check(BenchmarkCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkCheckpointPinnedGuardRule(double Limit) : IBenchmarkCheckpointRule
{
    public List<string> Check(BenchmarkCheckpoint item)
    {
        if (item.Pinned && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkCheckpointBatchSizeRangeRule(int Min, int Max) : IBenchmarkCheckpointRule
{
    public List<string> Check(BenchmarkCheckpoint item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkCheckpointAllowedCodeRule(List<string> Allowed) : IBenchmarkCheckpointRule
{
    public List<string> Check(BenchmarkCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkCheckpointNameLengthRule(int MaxLength) : IBenchmarkCheckpointRule
{
    public List<string> Check(BenchmarkCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkCheckpointRequiredNameRule(string Message) : IBenchmarkCheckpointRule
{
    public List<string> Check(BenchmarkCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark checkpoint.</summary>
public sealed record BenchmarkCheckpointValidator(List<IBenchmarkCheckpointRule> Rules)
{
    public List<string> Validate(BenchmarkCheckpoint item)
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

    public void RequireValid(BenchmarkCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkCheckpoint> items)
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

/// <summary>Functions of benchmark checkpoint rules.</summary>
public static class BenchmarkCheckpointRules
{
    public static BenchmarkCheckpointValidator DefaultBenchmarkCheckpointValidator()
    {
        return new BenchmarkCheckpointValidator(
            new List<IBenchmarkCheckpointRule>
            {
                new BenchmarkCheckpointCodePrefixRule("REF"),
                new BenchmarkCheckpointPinnedGuardRule(0.0),
                new BenchmarkCheckpointBatchSizeRangeRule(80, 1000),
                new BenchmarkCheckpointAllowedCodeRule(new List<string> { "lenient" }),
                new BenchmarkCheckpointNameLengthRule(80),
                new BenchmarkCheckpointRequiredNameRule("benchmark checkpoint needs an entry"),
            });
    }
}
