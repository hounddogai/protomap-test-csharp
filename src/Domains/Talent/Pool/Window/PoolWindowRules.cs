namespace Acme.Workforce.Talent.Pool.Window;

/// <summary>A rule a pool window must pass. Check returns its problems, or none.</summary>
public interface IPoolWindowRule
{
    List<string> Check(PoolWindow item);
}

public sealed record PoolWindowCodeLengthRule(int MaxLength) : IPoolWindowRule
{
    public List<string> Check(PoolWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolWindowRequiredManagerIdRule(string Message) : IPoolWindowRule
{
    public List<string> Check(PoolWindow item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolWindowManagerIdLengthRule(int MaxLength) : IPoolWindowRule
{
    public List<string> Check(PoolWindow item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolWindowHoursRangeRule(double Min, double Max) : IPoolWindowRule
{
    public List<string> Check(PoolWindow item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PoolWindowVisibleGuardRule(double Limit) : IPoolWindowRule
{
    public List<string> Check(PoolWindow item)
    {
        if (item.Visible && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PoolWindowStartsAtRangeRule(int Min, int Max) : IPoolWindowRule
{
    public List<string> Check(PoolWindow item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pool window.</summary>
public sealed record PoolWindowValidator(List<IPoolWindowRule> Rules)
{
    public List<string> Validate(PoolWindow item)
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

    public void RequireValid(PoolWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolWindow> items)
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

/// <summary>Functions of pool window rules.</summary>
public static class PoolWindowRules
{
    public static PoolWindowValidator DefaultPoolWindowValidator()
    {
        return new PoolWindowValidator(
            new List<IPoolWindowRule>
            {
                new PoolWindowCodeLengthRule(80),
                new PoolWindowRequiredManagerIdRule("pool window needs a value"),
                new PoolWindowManagerIdLengthRule(1000),
                new PoolWindowHoursRangeRule(2.5, 3446.4),
                new PoolWindowVisibleGuardRule(1.4),
                new PoolWindowStartsAtRangeRule(16, 120),
            });
    }
}
