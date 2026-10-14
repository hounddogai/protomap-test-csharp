namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling benchmark adjustment must pass. Check returns its problems, or none.</summary>
public interface IRollingBenchmarkAdjustmentRule
{
    List<string> Check(RollingBenchmarkAdjustment item);
}

public sealed record RollingBenchmarkAdjustmentCodePrefixRule(string Prefix) : IRollingBenchmarkAdjustmentRule
{
    public List<string> Check(RollingBenchmarkAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkAdjustmentAllowedLocationRule(List<string> Allowed) : IRollingBenchmarkAdjustmentRule
{
    public List<string> Check(RollingBenchmarkAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is expired" };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkAdjustmentCodeLengthRule(int MaxLength) : IRollingBenchmarkAdjustmentRule
{
    public List<string> Check(RollingBenchmarkAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBenchmarkAdjustmentRetriesRangeRule(int Min, int Max) : IRollingBenchmarkAdjustmentRule
{
    public List<string> Check(RollingBenchmarkAdjustment item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingBenchmarkAdjustmentTitleLengthRule(int MaxLength) : IRollingBenchmarkAdjustmentRule
{
    public List<string> Check(RollingBenchmarkAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling benchmark adjustment.</summary>
public sealed record RollingBenchmarkAdjustmentValidator(List<IRollingBenchmarkAdjustmentRule> Rules)
{
    public List<string> Validate(RollingBenchmarkAdjustment item)
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

    public void RequireValid(RollingBenchmarkAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBenchmarkAdjustment> items)
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

/// <summary>Functions of rolling benchmark adjustment rules.</summary>
public static class RollingBenchmarkAdjustmentRules
{
    public static RollingBenchmarkAdjustmentValidator DefaultRollingBenchmarkAdjustmentValidator()
    {
        return new RollingBenchmarkAdjustmentValidator(
            new List<IRollingBenchmarkAdjustmentRule>
            {
                new RollingBenchmarkAdjustmentCodePrefixRule("Q"),
                new RollingBenchmarkAdjustmentAllowedLocationRule(new List<string> { "batched", "adaptive" }),
                new RollingBenchmarkAdjustmentCodeLengthRule(255),
                new RollingBenchmarkAdjustmentRetriesRangeRule(16, 3600),
                new RollingBenchmarkAdjustmentTitleLengthRule(80),
            });
    }
}
