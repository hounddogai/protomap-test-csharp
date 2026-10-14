namespace Acme.Workforce.Talent.Pool.Quota;

/// <summary>A rule a pool quota must pass. Check returns its problems, or none.</summary>
public interface IPoolQuotaRule
{
    List<string> Check(PoolQuota item);
}

public sealed record PoolQuotaMaximumRangeRule(double Min, double Max) : IPoolQuotaRule
{
    public List<string> Check(PoolQuota item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolQuotaCeilingRangeRule(double Min, double Max) : IPoolQuotaRule
{
    public List<string> Check(PoolQuota item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolQuotaQuantityRangeRule(int Min, int Max) : IPoolQuotaRule
{
    public List<string> Check(PoolQuota item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PoolQuotaRequiredCodeRule(string Message) : IPoolQuotaRule
{
    public List<string> Check(PoolQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool quota.</summary>
public sealed record PoolQuotaValidator(List<IPoolQuotaRule> Rules)
{
    public List<string> Validate(PoolQuota item)
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

    public void RequireValid(PoolQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolQuota> items)
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

/// <summary>Functions of pool quota rules.</summary>
public static class PoolQuotaRules
{
    public static PoolQuotaValidator DefaultPoolQuotaValidator()
    {
        return new PoolQuotaValidator(
            new List<IPoolQuotaRule>
            {
                new PoolQuotaMaximumRangeRule(7.0, 1260.8),
                new PoolQuotaCeilingRangeRule(2.2, 1239.6),
                new PoolQuotaQuantityRangeRule(120, 1000),
                new PoolQuotaRequiredCodeRule("pool quota needs an entry"),
            });
    }
}
