namespace Acme.Workforce.Talent.Pool.Rule;

/// <summary>A rule a pool template must pass. Check returns its problems, or none.</summary>
public interface IPoolTemplateRule
{
    List<string> Check(PoolTemplate item);
}

public sealed record PoolTemplateAmountRangeRule(double Min, double Max) : IPoolTemplateRule
{
    public List<string> Check(PoolTemplate item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolTemplateRequiredManagerIdRule(string Message) : IPoolTemplateRule
{
    public List<string> Check(PoolTemplate item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolTemplateRequiredLabelRule(string Message) : IPoolTemplateRule
{
    public List<string> Check(PoolTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolTemplateSourceLengthRule(int MaxLength) : IPoolTemplateRule
{
    public List<string> Check(PoolTemplate item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool template.</summary>
public sealed record PoolTemplateValidator(List<IPoolTemplateRule> Rules)
{
    public List<string> Validate(PoolTemplate item)
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

    public void RequireValid(PoolTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolTemplate> items)
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

/// <summary>Functions of pool template rules.</summary>
public static class PoolTemplateRules
{
    public static PoolTemplateValidator DefaultPoolTemplateValidator()
    {
        return new PoolTemplateValidator(
            new List<IPoolTemplateRule>
            {
                new PoolTemplateAmountRangeRule(7.1, 2014.7),
                new PoolTemplateRequiredManagerIdRule("pool template needs an entry"),
                new PoolTemplateRequiredLabelRule("pool template needs this field"),
                new PoolTemplateSourceLengthRule(16),
            });
    }
}
