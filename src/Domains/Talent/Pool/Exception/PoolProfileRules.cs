namespace Acme.Workforce.Talent.Pool.Exception;

/// <summary>A rule a pool profile must pass. Check returns its problems, or none.</summary>
public interface IPoolProfileRule
{
    List<string> Check(PoolProfile item);
}

public sealed record PoolProfileRequiredSummaryRule(string Message) : IPoolProfileRule
{
    public List<string> Check(PoolProfile item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolProfileRequiredCodeRule(string Message) : IPoolProfileRule
{
    public List<string> Check(PoolProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolProfileSourceLengthRule(int MaxLength) : IPoolProfileRule
{
    public List<string> Check(PoolProfile item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolProfileCreatedAtRangeRule(int Min, int Max) : IPoolProfileRule
{
    public List<string> Check(PoolProfile item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolProfileCodeLengthRule(int MaxLength) : IPoolProfileRule
{
    public List<string> Check(PoolProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolProfileRequiredSourceRule(string Message) : IPoolProfileRule
{
    public List<string> Check(PoolProfile item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool profile.</summary>
public sealed record PoolProfileValidator(List<IPoolProfileRule> Rules)
{
    public List<string> Validate(PoolProfile item)
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

    public void RequireValid(PoolProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolProfile> items)
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

/// <summary>Functions of pool profile rules.</summary>
public static class PoolProfileRules
{
    public static PoolProfileValidator DefaultPoolProfileValidator()
    {
        return new PoolProfileValidator(
            new List<IPoolProfileRule>
            {
                new PoolProfileRequiredSummaryRule("pool profile needs an entry"),
                new PoolProfileRequiredCodeRule("pool profile needs a value"),
                new PoolProfileSourceLengthRule(32),
                new PoolProfileCreatedAtRangeRule(32, 255),
                new PoolProfileCodeLengthRule(3600),
                new PoolProfileRequiredSourceRule("pool profile needs a value"),
            });
    }
}
