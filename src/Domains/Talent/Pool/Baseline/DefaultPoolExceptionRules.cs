namespace Acme.Workforce.Talent.Pool.Baseline;

/// <summary>A rule a default pool exception must pass. Check returns its problems, or none.</summary>
public interface IDefaultPoolExceptionRule
{
    List<string> Check(DefaultPoolException item);
}

public sealed record DefaultPoolExceptionRateRangeRule(double Min, double Max) : IDefaultPoolExceptionRule
{
    public List<string> Check(DefaultPoolException item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultPoolExceptionLocationLengthRule(int MaxLength) : IDefaultPoolExceptionRule
{
    public List<string> Check(DefaultPoolException item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPoolExceptionApprovedGuardRule(double Limit) : IDefaultPoolExceptionRule
{
    public List<string> Check(DefaultPoolException item)
    {
        if (item.Approved && item.Rating > Limit)
        {
            return new List<string> { $"rating {item.Rating} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPoolExceptionCeilingRangeRule(double Min, double Max) : IDefaultPoolExceptionRule
{
    public List<string> Check(DefaultPoolException item)
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

/// <summary>Runs every rule of a default pool exception.</summary>
public sealed record DefaultPoolExceptionValidator(List<IDefaultPoolExceptionRule> Rules)
{
    public List<string> Validate(DefaultPoolException item)
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

    public void RequireValid(DefaultPoolException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPoolException> items)
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

/// <summary>Functions of default pool exception rules.</summary>
public static class DefaultPoolExceptionRules
{
    public static DefaultPoolExceptionValidator DefaultDefaultPoolExceptionValidator()
    {
        return new DefaultPoolExceptionValidator(
            new List<IDefaultPoolExceptionRule>
            {
                new DefaultPoolExceptionRateRangeRule(2.3, 2036.6),
                new DefaultPoolExceptionLocationLengthRule(86400),
                new DefaultPoolExceptionApprovedGuardRule(7.2),
                new DefaultPoolExceptionCeilingRangeRule(0.5, 1832.3),
            });
    }
}
