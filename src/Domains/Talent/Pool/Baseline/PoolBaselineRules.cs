namespace Acme.Workforce.Talent.Pool.Baseline;

/// <summary>A rule a pool baseline must pass. Check returns its problems, or none.</summary>
public interface IPoolBaselineRule
{
    List<string> Check(PoolBaseline item);
}

public sealed record PoolBaselineRequiredTitleRule(string Message) : IPoolBaselineRule
{
    public List<string> Check(PoolBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolBaselineRequiredEmployeeIdRule(string Message) : IPoolBaselineRule
{
    public List<string> Check(PoolBaseline item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolBaselineCodeLengthRule(int MaxLength) : IPoolBaselineRule
{
    public List<string> Check(PoolBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolBaselineRequiredCodeRule(string Message) : IPoolBaselineRule
{
    public List<string> Check(PoolBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolBaselinePriorityRangeRule(int Min, int Max) : IPoolBaselineRule
{
    public List<string> Check(PoolBaseline item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pool baseline.</summary>
public sealed record PoolBaselineValidator(List<IPoolBaselineRule> Rules)
{
    public List<string> Validate(PoolBaseline item)
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

    public void RequireValid(PoolBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolBaseline> items)
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

/// <summary>Functions of pool baseline rules.</summary>
public static class PoolBaselineRules
{
    public static PoolBaselineValidator DefaultPoolBaselineValidator()
    {
        return new PoolBaselineValidator(
            new List<IPoolBaselineRule>
            {
                new PoolBaselineRequiredTitleRule("pool baseline needs a value"),
                new PoolBaselineRequiredEmployeeIdRule("pool baseline needs an entry"),
                new PoolBaselineCodeLengthRule(16),
                new PoolBaselineRequiredCodeRule("pool baseline needs this field"),
                new PoolBaselinePriorityRangeRule(120, 86400),
            });
    }
}
