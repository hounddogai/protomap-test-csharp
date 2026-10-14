namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>A rule a rolling access badge request must pass. Check returns its problems, or none.</summary>
public interface IRollingAccessBadgeRequestRule
{
    List<string> Check(RollingAccessBadgeRequest item);
}

public sealed record RollingAccessBadgeRequestRequiredCodeRule(string Message) : IRollingAccessBadgeRequestRule
{
    public List<string> Check(RollingAccessBadgeRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAccessBadgeRequestEndsAtRangeRule(int Min, int Max) : IRollingAccessBadgeRequestRule
{
    public List<string> Check(RollingAccessBadgeRequest item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingAccessBadgeRequestMaximumRangeRule(double Min, double Max) : IRollingAccessBadgeRequestRule
{
    public List<string> Check(RollingAccessBadgeRequest item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingAccessBadgeRequestCodeLengthRule(int MaxLength) : IRollingAccessBadgeRequestRule
{
    public List<string> Check(RollingAccessBadgeRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAccessBadgeRequestWindowRule(int MaxSpan) : IRollingAccessBadgeRequestRule
{
    public List<string> Check(RollingAccessBadgeRequest item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling access badge request.</summary>
public sealed record RollingAccessBadgeRequestValidator(List<IRollingAccessBadgeRequestRule> Rules)
{
    public List<string> Validate(RollingAccessBadgeRequest item)
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

    public void RequireValid(RollingAccessBadgeRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAccessBadgeRequest> items)
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

/// <summary>Functions of rolling access badge request rules.</summary>
public static class RollingAccessBadgeRequestRules
{
    public static RollingAccessBadgeRequestValidator DefaultRollingAccessBadgeRequestValidator()
    {
        return new RollingAccessBadgeRequestValidator(
            new List<IRollingAccessBadgeRequestRule>
            {
                new RollingAccessBadgeRequestRequiredCodeRule("rolling access badge request needs this field"),
                new RollingAccessBadgeRequestEndsAtRangeRule(80, 120),
                new RollingAccessBadgeRequestMaximumRangeRule(3.7, 2189.3),
                new RollingAccessBadgeRequestCodeLengthRule(1000),
                new RollingAccessBadgeRequestWindowRule(120),
            });
    }
}
