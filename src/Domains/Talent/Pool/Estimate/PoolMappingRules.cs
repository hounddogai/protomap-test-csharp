namespace Acme.Workforce.Talent.Pool.Estimate;

/// <summary>A rule a pool mapping must pass. Check returns its problems, or none.</summary>
public interface IPoolMappingRule
{
    List<string> Check(PoolMapping item);
}

public sealed record PoolMappingRequiredNameRule(string Message) : IPoolMappingRule
{
    public List<string> Check(PoolMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolMappingJobCodeLengthRule(int MaxLength) : IPoolMappingRule
{
    public List<string> Check(PoolMapping item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolMappingRequiredCodeRule(string Message) : IPoolMappingRule
{
    public List<string> Check(PoolMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolMappingVisibleGuardRule(double Limit) : IPoolMappingRule
{
    public List<string> Check(PoolMapping item)
    {
        if (item.Visible && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PoolMappingRevisionRangeRule(int Min, int Max) : IPoolMappingRule
{
    public List<string> Check(PoolMapping item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolMappingCostRangeRule(double Min, double Max) : IPoolMappingRule
{
    public List<string> Check(PoolMapping item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pool mapping.</summary>
public sealed record PoolMappingValidator(List<IPoolMappingRule> Rules)
{
    public List<string> Validate(PoolMapping item)
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

    public void RequireValid(PoolMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolMapping> items)
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

/// <summary>Functions of pool mapping rules.</summary>
public static class PoolMappingRules
{
    public static PoolMappingValidator DefaultPoolMappingValidator()
    {
        return new PoolMappingValidator(
            new List<IPoolMappingRule>
            {
                new PoolMappingRequiredNameRule("pool mapping needs an entry"),
                new PoolMappingJobCodeLengthRule(3600),
                new PoolMappingRequiredCodeRule("pool mapping needs an entry"),
                new PoolMappingVisibleGuardRule(0.3),
                new PoolMappingRevisionRangeRule(32, 255),
                new PoolMappingCostRangeRule(7.6, 2133.3),
            });
    }
}
