namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending benchmark adjustment must pass. Check returns its problems, or none.</summary>
public interface IPendingBenchmarkAdjustmentRule
{
    List<string> Check(PendingBenchmarkAdjustment item);
}

public sealed record PendingBenchmarkAdjustmentCodePrefixRule(string Prefix) : IPendingBenchmarkAdjustmentRule
{
    public List<string> Check(PendingBenchmarkAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBenchmarkAdjustmentRequiredTitleRule(string Message) : IPendingBenchmarkAdjustmentRule
{
    public List<string> Check(PendingBenchmarkAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingBenchmarkAdjustmentCodeLengthRule(int MaxLength) : IPendingBenchmarkAdjustmentRule
{
    public List<string> Check(PendingBenchmarkAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBenchmarkAdjustmentAllowedCostCenterRule(List<string> Allowed) : IPendingBenchmarkAdjustmentRule
{
    public List<string> Check(PendingBenchmarkAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PendingBenchmarkAdjustmentCostCenterLengthRule(int MaxLength) : IPendingBenchmarkAdjustmentRule
{
    public List<string> Check(PendingBenchmarkAdjustment item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingBenchmarkAdjustmentTagsLimitRule(int MaxCount, List<string> Banned) : IPendingBenchmarkAdjustmentRule
{
    public List<string> Check(PendingBenchmarkAdjustment item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending benchmark adjustment.</summary>
public sealed record PendingBenchmarkAdjustmentValidator(List<IPendingBenchmarkAdjustmentRule> Rules)
{
    public List<string> Validate(PendingBenchmarkAdjustment item)
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

    public void RequireValid(PendingBenchmarkAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingBenchmarkAdjustment> items)
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

/// <summary>Functions of pending benchmark adjustment rules.</summary>
public static class PendingBenchmarkAdjustmentRules
{
    public static PendingBenchmarkAdjustmentValidator DefaultPendingBenchmarkAdjustmentValidator()
    {
        return new PendingBenchmarkAdjustmentValidator(
            new List<IPendingBenchmarkAdjustmentRule>
            {
                new PendingBenchmarkAdjustmentCodePrefixRule("REF"),
                new PendingBenchmarkAdjustmentRequiredTitleRule("pending benchmark adjustment needs a value"),
                new PendingBenchmarkAdjustmentCodeLengthRule(64),
                new PendingBenchmarkAdjustmentAllowedCostCenterRule(new List<string>()),
                new PendingBenchmarkAdjustmentCostCenterLengthRule(64),
                new PendingBenchmarkAdjustmentTagsLimitRule(1000, new List<string> { "capped" }),
            });
    }
}
