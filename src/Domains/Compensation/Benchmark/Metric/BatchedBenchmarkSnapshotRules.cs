namespace Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>A rule a batched benchmark snapshot must pass. Check returns its problems, or none.</summary>
public interface IBatchedBenchmarkSnapshotRule
{
    List<string> Check(BatchedBenchmarkSnapshot item);
}

public sealed record BatchedBenchmarkSnapshotNameLengthRule(int MaxLength) : IBatchedBenchmarkSnapshotRule
{
    public List<string> Check(BatchedBenchmarkSnapshot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBenchmarkSnapshotCodePrefixRule(string Prefix) : IBatchedBenchmarkSnapshotRule
{
    public List<string> Check(BatchedBenchmarkSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBenchmarkSnapshotRequiredCodeRule(string Message) : IBatchedBenchmarkSnapshotRule
{
    public List<string> Check(BatchedBenchmarkSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched benchmark snapshot.</summary>
public sealed record BatchedBenchmarkSnapshotValidator(List<IBatchedBenchmarkSnapshotRule> Rules)
{
    public List<string> Validate(BatchedBenchmarkSnapshot item)
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

    public void RequireValid(BatchedBenchmarkSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedBenchmarkSnapshot> items)
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

/// <summary>Functions of batched benchmark snapshot rules.</summary>
public static class BatchedBenchmarkSnapshotRules
{
    public static BatchedBenchmarkSnapshotValidator DefaultBatchedBenchmarkSnapshotValidator()
    {
        return new BatchedBenchmarkSnapshotValidator(
            new List<IBatchedBenchmarkSnapshotRule>
            {
                new BatchedBenchmarkSnapshotNameLengthRule(255),
                new BatchedBenchmarkSnapshotCodePrefixRule("X"),
                new BatchedBenchmarkSnapshotRequiredCodeRule("batched benchmark snapshot needs an entry"),
            });
    }
}
