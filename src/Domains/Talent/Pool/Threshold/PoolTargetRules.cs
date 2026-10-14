namespace Acme.Workforce.Talent.Pool.Threshold;

/// <summary>A rule a pool target must pass. Check returns its problems, or none.</summary>
public interface IPoolTargetRule
{
    List<string> Check(PoolTarget item);
}

public sealed record PoolTargetRequiredNameRule(string Message) : IPoolTargetRule
{
    public List<string> Check(PoolTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolTargetRequiredLocationRule(string Message) : IPoolTargetRule
{
    public List<string> Check(PoolTarget item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolTargetCodePrefixRule(string Prefix) : IPoolTargetRule
{
    public List<string> Check(PoolTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolTargetRequiredCodeRule(string Message) : IPoolTargetRule
{
    public List<string> Check(PoolTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolTargetNameLengthRule(int MaxLength) : IPoolTargetRule
{
    public List<string> Check(PoolTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool target.</summary>
public sealed record PoolTargetValidator(List<IPoolTargetRule> Rules)
{
    public List<string> Validate(PoolTarget item)
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

    public void RequireValid(PoolTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolTarget> items)
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

/// <summary>Functions of pool target rules.</summary>
public static class PoolTargetRules
{
    public static PoolTargetValidator DefaultPoolTargetValidator()
    {
        return new PoolTargetValidator(
            new List<IPoolTargetRule>
            {
                new PoolTargetRequiredNameRule("pool target needs an entry"),
                new PoolTargetRequiredLocationRule("pool target needs this field"),
                new PoolTargetCodePrefixRule("OPS"),
                new PoolTargetRequiredCodeRule("pool target needs an entry"),
                new PoolTargetNameLengthRule(255),
            });
    }
}
