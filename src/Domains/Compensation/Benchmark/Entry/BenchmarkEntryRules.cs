namespace Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>A rule a benchmark entry must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkEntryRule
{
    List<string> Check(BenchmarkEntry item);
}

public sealed record BenchmarkEntryBatchSizeRangeRule(int Min, int Max) : IBenchmarkEntryRule
{
    public List<string> Check(BenchmarkEntry item)
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

public sealed record BenchmarkEntryCodePrefixRule(string Prefix) : IBenchmarkEntryRule
{
    public List<string> Check(BenchmarkEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkEntryRequiredCodeRule(string Message) : IBenchmarkEntryRule
{
    public List<string> Check(BenchmarkEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkEntryCodeLengthRule(int MaxLength) : IBenchmarkEntryRule
{
    public List<string> Check(BenchmarkEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark entry.</summary>
public sealed record BenchmarkEntryValidator(List<IBenchmarkEntryRule> Rules)
{
    public List<string> Validate(BenchmarkEntry item)
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

    public void RequireValid(BenchmarkEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkEntry> items)
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

/// <summary>Functions of benchmark entry rules.</summary>
public static class BenchmarkEntryRules
{
    public static BenchmarkEntryValidator DefaultBenchmarkEntryValidator()
    {
        return new BenchmarkEntryValidator(
            new List<IBenchmarkEntryRule>
            {
                new BenchmarkEntryBatchSizeRangeRule(80, 1000),
                new BenchmarkEntryCodePrefixRule("Q"),
                new BenchmarkEntryRequiredCodeRule("benchmark entry needs this field"),
                new BenchmarkEntryCodeLengthRule(80),
            });
    }
}
