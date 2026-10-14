namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark request must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkRequestRule
{
    List<string> Check(BenchmarkRequest item);
}

public sealed record BenchmarkRequestRequiredNameRule(string Message) : IBenchmarkRequestRule
{
    public List<string> Check(BenchmarkRequest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkRequestAllowedNameRule(List<string> Allowed) : IBenchmarkRequestRule
{
    public List<string> Check(BenchmarkRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkRequestCodePrefixRule(string Prefix) : IBenchmarkRequestRule
{
    public List<string> Check(BenchmarkRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkRequestNotesLengthRule(int MaxLength) : IBenchmarkRequestRule
{
    public List<string> Check(BenchmarkRequest item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark request.</summary>
public sealed record BenchmarkRequestValidator(List<IBenchmarkRequestRule> Rules)
{
    public List<string> Validate(BenchmarkRequest item)
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

    public void RequireValid(BenchmarkRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkRequest> items)
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

/// <summary>Functions of benchmark request rules.</summary>
public static class BenchmarkRequestRules
{
    public static BenchmarkRequestValidator DefaultBenchmarkRequestValidator()
    {
        return new BenchmarkRequestValidator(
            new List<IBenchmarkRequestRule>
            {
                new BenchmarkRequestRequiredNameRule("benchmark request needs an entry"),
                new BenchmarkRequestAllowedNameRule(new List<string> { "default", "batched", "capped" }),
                new BenchmarkRequestCodePrefixRule("Q"),
                new BenchmarkRequestNotesLengthRule(64),
            });
    }
}
