namespace Acme.Workforce.Talent.Pool.Estimate;

/// <summary>A rule a pool estimate must pass. Check returns its problems, or none.</summary>
public interface IPoolEstimateRule
{
    List<string> Check(PoolEstimate item);
}

public sealed record PoolEstimateRegionLengthRule(int MaxLength) : IPoolEstimateRule
{
    public List<string> Check(PoolEstimate item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolEstimateRequiredTitleRule(string Message) : IPoolEstimateRule
{
    public List<string> Check(PoolEstimate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolEstimateTitleLengthRule(int MaxLength) : IPoolEstimateRule
{
    public List<string> Check(PoolEstimate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolEstimateRequiredRegionRule(string Message) : IPoolEstimateRule
{
    public List<string> Check(PoolEstimate item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool estimate.</summary>
public sealed record PoolEstimateValidator(List<IPoolEstimateRule> Rules)
{
    public List<string> Validate(PoolEstimate item)
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

    public void RequireValid(PoolEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolEstimate> items)
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

/// <summary>Functions of pool estimate rules.</summary>
public static class PoolEstimateRules
{
    public static PoolEstimateValidator DefaultPoolEstimateValidator()
    {
        return new PoolEstimateValidator(
            new List<IPoolEstimateRule>
            {
                new PoolEstimateRegionLengthRule(16),
                new PoolEstimateRequiredTitleRule("pool estimate needs this field"),
                new PoolEstimateTitleLengthRule(255),
                new PoolEstimateRequiredRegionRule("pool estimate needs a value"),
            });
    }
}
