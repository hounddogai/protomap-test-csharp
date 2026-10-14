namespace Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>A rule a access badge exception must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeExceptionRule
{
    List<string> Check(AccessBadgeException item);
}

public sealed record AccessBadgeExceptionTitleLengthRule(int MaxLength) : IAccessBadgeExceptionRule
{
    public List<string> Check(AccessBadgeException item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeExceptionRequiredCodeRule(string Message) : IAccessBadgeExceptionRule
{
    public List<string> Check(AccessBadgeException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeExceptionDurationMinutesRangeRule(int Min, int Max) : IAccessBadgeExceptionRule
{
    public List<string> Check(AccessBadgeException item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a access badge exception.</summary>
public sealed record AccessBadgeExceptionValidator(List<IAccessBadgeExceptionRule> Rules)
{
    public List<string> Validate(AccessBadgeException item)
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

    public void RequireValid(AccessBadgeException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeException> items)
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

/// <summary>Functions of access badge exception rules.</summary>
public static class AccessBadgeExceptionRules
{
    public static AccessBadgeExceptionValidator DefaultAccessBadgeExceptionValidator()
    {
        return new AccessBadgeExceptionValidator(
            new List<IAccessBadgeExceptionRule>
            {
                new AccessBadgeExceptionTitleLengthRule(86400),
                new AccessBadgeExceptionRequiredCodeRule("access badge exception needs an entry"),
                new AccessBadgeExceptionDurationMinutesRangeRule(64, 86400),
            });
    }
}
