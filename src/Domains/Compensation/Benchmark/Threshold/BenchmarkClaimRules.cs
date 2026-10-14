namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a benchmark claim must pass. Check returns its problems, or none.</summary>
public interface IBenchmarkClaimRule
{
    List<string> Check(BenchmarkClaim item);
}

public sealed record BenchmarkClaimAllowedCodeRule(List<string> Allowed) : IBenchmarkClaimRule
{
    public List<string> Check(BenchmarkClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkClaimCodeLengthRule(int MaxLength) : IBenchmarkClaimRule
{
    public List<string> Check(BenchmarkClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkClaimAmountRangeRule(double Min, double Max) : IBenchmarkClaimRule
{
    public List<string> Check(BenchmarkClaim item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BenchmarkClaimArchivedGuardRule(int Limit) : IBenchmarkClaimRule
{
    public List<string> Check(BenchmarkClaim item)
    {
        if (item.Archived && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record BenchmarkClaimTimezoneLengthRule(int MaxLength) : IBenchmarkClaimRule
{
    public List<string> Check(BenchmarkClaim item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a benchmark claim.</summary>
public sealed record BenchmarkClaimValidator(List<IBenchmarkClaimRule> Rules)
{
    public List<string> Validate(BenchmarkClaim item)
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

    public void RequireValid(BenchmarkClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BenchmarkClaim> items)
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

/// <summary>Functions of benchmark claim rules.</summary>
public static class BenchmarkClaimRules
{
    public static BenchmarkClaimValidator DefaultBenchmarkClaimValidator()
    {
        return new BenchmarkClaimValidator(
            new List<IBenchmarkClaimRule>
            {
                new BenchmarkClaimAllowedCodeRule(new List<string> { "strict", "fallback", "scheduled" }),
                new BenchmarkClaimCodeLengthRule(32),
                new BenchmarkClaimAmountRangeRule(3.8, 2185.3),
                new BenchmarkClaimArchivedGuardRule(120),
                new BenchmarkClaimTimezoneLengthRule(255),
            });
    }
}
