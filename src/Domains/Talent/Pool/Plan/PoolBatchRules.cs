namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pool batch must pass. Check returns its problems, or none.</summary>
public interface IPoolBatchRule
{
    List<string> Check(PoolBatch item);
}

public sealed record PoolBatchAllowedLocationRule(List<string> Allowed) : IPoolBatchRule
{
    public List<string> Check(PoolBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PoolBatchRequiredLabelRule(string Message) : IPoolBatchRule
{
    public List<string> Check(PoolBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolBatchCodePrefixRule(string Prefix) : IPoolBatchRule
{
    public List<string> Check(PoolBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolBatchRequiredNameRule(string Message) : IPoolBatchRule
{
    public List<string> Check(PoolBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool batch.</summary>
public sealed record PoolBatchValidator(List<IPoolBatchRule> Rules)
{
    public List<string> Validate(PoolBatch item)
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

    public void RequireValid(PoolBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolBatch> items)
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

/// <summary>Functions of pool batch rules.</summary>
public static class PoolBatchRules
{
    public static PoolBatchValidator DefaultPoolBatchValidator()
    {
        return new PoolBatchValidator(
            new List<IPoolBatchRule>
            {
                new PoolBatchAllowedLocationRule(new List<string> { "tiered", "fallback" }),
                new PoolBatchRequiredLabelRule("pool batch needs a value"),
                new PoolBatchCodePrefixRule("ACM"),
                new PoolBatchRequiredNameRule("pool batch needs an entry"),
            });
    }
}
