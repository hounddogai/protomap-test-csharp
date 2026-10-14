namespace Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>A rule a primary benchmark claim must pass. Check returns its problems, or none.</summary>
public interface IPrimaryBenchmarkClaimRule
{
    List<string> Check(PrimaryBenchmarkClaim item);
}

public sealed record PrimaryBenchmarkClaimTitleLengthRule(int MaxLength) : IPrimaryBenchmarkClaimRule
{
    public List<string> Check(PrimaryBenchmarkClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBenchmarkClaimCodePrefixRule(string Prefix) : IPrimaryBenchmarkClaimRule
{
    public List<string> Check(PrimaryBenchmarkClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBenchmarkClaimLocationLengthRule(int MaxLength) : IPrimaryBenchmarkClaimRule
{
    public List<string> Check(PrimaryBenchmarkClaim item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary benchmark claim.</summary>
public sealed record PrimaryBenchmarkClaimValidator(List<IPrimaryBenchmarkClaimRule> Rules)
{
    public List<string> Validate(PrimaryBenchmarkClaim item)
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

    public void RequireValid(PrimaryBenchmarkClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryBenchmarkClaim> items)
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

/// <summary>Functions of primary benchmark claim rules.</summary>
public static class PrimaryBenchmarkClaimRules
{
    public static PrimaryBenchmarkClaimValidator DefaultPrimaryBenchmarkClaimValidator()
    {
        return new PrimaryBenchmarkClaimValidator(
            new List<IPrimaryBenchmarkClaimRule>
            {
                new PrimaryBenchmarkClaimTitleLengthRule(64),
                new PrimaryBenchmarkClaimCodePrefixRule("OPS"),
                new PrimaryBenchmarkClaimLocationLengthRule(1000),
            });
    }
}
