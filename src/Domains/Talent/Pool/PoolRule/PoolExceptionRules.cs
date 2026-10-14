namespace Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>A rule a pool exception must pass. Check returns its problems, or none.</summary>
public interface IPoolExceptionRule
{
    List<string> Check(PoolException item);
}

public sealed record PoolExceptionRequiredTitleRule(string Message) : IPoolExceptionRule
{
    public List<string> Check(PoolException item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolExceptionRequiredTimezoneRule(string Message) : IPoolExceptionRule
{
    public List<string> Check(PoolException item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolExceptionEmployeeIdLengthRule(int MaxLength) : IPoolExceptionRule
{
    public List<string> Check(PoolException item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolExceptionQuantityRangeRule(int Min, int Max) : IPoolExceptionRule
{
    public List<string> Check(PoolException item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolExceptionTitleLengthRule(int MaxLength) : IPoolExceptionRule
{
    public List<string> Check(PoolException item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool exception.</summary>
public sealed record PoolExceptionValidator(List<IPoolExceptionRule> Rules)
{
    public List<string> Validate(PoolException item)
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

    public void RequireValid(PoolException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolException> items)
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

/// <summary>Functions of pool exception rules.</summary>
public static class PoolExceptionRules
{
    public static PoolExceptionValidator DefaultPoolExceptionValidator()
    {
        return new PoolExceptionValidator(
            new List<IPoolExceptionRule>
            {
                new PoolExceptionRequiredTitleRule("pool exception needs an entry"),
                new PoolExceptionRequiredTimezoneRule("pool exception needs a value"),
                new PoolExceptionEmployeeIdLengthRule(32),
                new PoolExceptionQuantityRangeRule(64, 3600),
                new PoolExceptionTitleLengthRule(3600),
            });
    }
}
