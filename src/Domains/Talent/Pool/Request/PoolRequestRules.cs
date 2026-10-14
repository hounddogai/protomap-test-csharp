namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pool request must pass. Check returns its problems, or none.</summary>
public interface IPoolRequestRule
{
    List<string> Check(PoolRequest item);
}

public sealed record PoolRequestCodePrefixRule(string Prefix) : IPoolRequestRule
{
    public List<string> Check(PoolRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolRequestCapacityRangeRule(int Min, int Max) : IPoolRequestRule
{
    public List<string> Check(PoolRequest item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolRequestCurrencyLengthRule(int MaxLength) : IPoolRequestRule
{
    public List<string> Check(PoolRequest item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolRequestAllowedNameRule(List<string> Allowed) : IPoolRequestRule
{
    public List<string> Check(PoolRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PoolRequestRequiredCodeRule(string Message) : IPoolRequestRule
{
    public List<string> Check(PoolRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool request.</summary>
public sealed record PoolRequestValidator(List<IPoolRequestRule> Rules)
{
    public List<string> Validate(PoolRequest item)
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

    public void RequireValid(PoolRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolRequest> items)
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

/// <summary>Functions of pool request rules.</summary>
public static class PoolRequestRules
{
    public static PoolRequestValidator DefaultPoolRequestValidator()
    {
        return new PoolRequestValidator(
            new List<IPoolRequestRule>
            {
                new PoolRequestCodePrefixRule("X"),
                new PoolRequestCapacityRangeRule(16, 86400),
                new PoolRequestCurrencyLengthRule(86400),
                new PoolRequestAllowedNameRule(new List<string> { "manual", "regional", "regional" }),
                new PoolRequestRequiredCodeRule("pool request needs a value"),
            });
    }
}
