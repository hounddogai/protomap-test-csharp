namespace Acme.Workforce.Talent.Pool.Assignment;

/// <summary>A rule a pool allocation must pass. Check returns its problems, or none.</summary>
public interface IPoolAllocationRule
{
    List<string> Check(PoolAllocation item);
}

public sealed record PoolAllocationLimitRangeRule(int Min, int Max) : IPoolAllocationRule
{
    public List<string> Check(PoolAllocation item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PoolAllocationCeilingRangeRule(double Min, double Max) : IPoolAllocationRule
{
    public List<string> Check(PoolAllocation item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PoolAllocationChannelLengthRule(int MaxLength) : IPoolAllocationRule
{
    public List<string> Check(PoolAllocation item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool allocation.</summary>
public sealed record PoolAllocationValidator(List<IPoolAllocationRule> Rules)
{
    public List<string> Validate(PoolAllocation item)
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

    public void RequireValid(PoolAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolAllocation> items)
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

/// <summary>Functions of pool allocation rules.</summary>
public static class PoolAllocationRules
{
    public static PoolAllocationValidator DefaultPoolAllocationValidator()
    {
        return new PoolAllocationValidator(
            new List<IPoolAllocationRule>
            {
                new PoolAllocationLimitRangeRule(16, 1000),
                new PoolAllocationCeilingRangeRule(6.0, 3490.5),
                new PoolAllocationChannelLengthRule(255),
            });
    }
}
