namespace Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>A rule a pool cycle must pass. Check returns its problems, or none.</summary>
public interface IPoolCycleRule
{
    List<string> Check(PoolCycle item);
}

public sealed record PoolCycleCodePrefixRule(string Prefix) : IPoolCycleRule
{
    public List<string> Check(PoolCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PoolCycleRetriesRangeRule(int Min, int Max) : IPoolCycleRule
{
    public List<string> Check(PoolCycle item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolCycleBudgetRangeRule(double Min, double Max) : IPoolCycleRule
{
    public List<string> Check(PoolCycle item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool cycle.</summary>
public sealed record PoolCycleValidator(List<IPoolCycleRule> Rules)
{
    public List<string> Validate(PoolCycle item)
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

    public void RequireValid(PoolCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolCycle> items)
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

/// <summary>Functions of pool cycle rules.</summary>
public static class PoolCycleRules
{
    public static PoolCycleValidator DefaultPoolCycleValidator()
    {
        return new PoolCycleValidator(
            new List<IPoolCycleRule>
            {
                new PoolCycleCodePrefixRule("OPS"),
                new PoolCycleRetriesRangeRule(3600, 86400),
                new PoolCycleBudgetRangeRule(5.3, 264.1),
            });
    }
}
