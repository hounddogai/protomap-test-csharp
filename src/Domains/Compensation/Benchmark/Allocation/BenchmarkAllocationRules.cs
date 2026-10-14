namespace Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>A rule a benchmark allocation must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkAllocationRule
{
    List<string> Check(BenchmarkAllocation item);
}

public sealed record BenchmarkAllocationRequiredCodeRule(string Message) : IBenchmarkAllocationRule
{
    public List<string> Check(BenchmarkAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkAllocationLabelLengthRule(int MaxLength) : IBenchmarkAllocationRule
{
    public List<string> Check(BenchmarkAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkAllocationCodePrefixRule(string Prefix) : IBenchmarkAllocationRule
{
    public List<string> Check(BenchmarkAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark allocation.</summary>
public sealed record BenchmarkAllocationValidator(List<IBenchmarkAllocationRule> Rules)
{
    public List<string> Validate(BenchmarkAllocation item)
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

    public void RequireValid(BenchmarkAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkAllocation> items)
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

/// <summary>Functions of benchmark allocation rules.</summary>
public static class BenchmarkAllocationRules
{
    public static BenchmarkAllocationValidator DefaultBenchmarkAllocationValidator()
    {
        return new BenchmarkAllocationValidator(
            new List<IBenchmarkAllocationRule>
            {
                new BenchmarkAllocationRequiredCodeRule("benchmark allocation needs a value"),
                new BenchmarkAllocationLabelLengthRule(120),
                new BenchmarkAllocationCodePrefixRule("ACM"),
            });
    }
}
