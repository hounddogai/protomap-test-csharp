namespace Acme.Workforce.Talent.Pool.Group;

/// <summary>A rule a pool override must pass. Check returns its problems, or none.</summary>
public interface IPoolOverrideRule
{
    List<string> Check(PoolOverride item);
}

public sealed record PoolOverrideCodePrefixRule(string Prefix) : IPoolOverrideRule
{
    public List<string> Check(PoolOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolOverrideAmountRangeRule(double Min, double Max) : IPoolOverrideRule
{
    public List<string> Check(PoolOverride item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolOverrideRequiredExternalRefRule(string Message) : IPoolOverrideRule
{
    public List<string> Check(PoolOverride item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolOverrideRequiredOwnerIdRule(string Message) : IPoolOverrideRule
{
    public List<string> Check(PoolOverride item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolOverrideBatchSizeRangeRule(int Min, int Max) : IPoolOverrideRule
{
    public List<string> Check(PoolOverride item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool override.</summary>
public sealed record PoolOverrideValidator(List<IPoolOverrideRule> Rules)
{
    public List<string> Validate(PoolOverride item)
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

    public void RequireValid(PoolOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolOverride> items)
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

/// <summary>Functions of pool override rules.</summary>
public static class PoolOverrideRules
{
    public static PoolOverrideValidator DefaultPoolOverrideValidator()
    {
        return new PoolOverrideValidator(
            new List<IPoolOverrideRule>
            {
                new PoolOverrideCodePrefixRule("ACM"),
                new PoolOverrideAmountRangeRule(9.2, 3203.3),
                new PoolOverrideRequiredExternalRefRule("pool override needs an entry"),
                new PoolOverrideRequiredOwnerIdRule("pool override needs a value"),
                new PoolOverrideBatchSizeRangeRule(255, 3600),
            });
    }
}
