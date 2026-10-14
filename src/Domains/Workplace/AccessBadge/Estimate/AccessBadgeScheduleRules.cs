namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

/// <summary>A rule a access badge schedule must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeScheduleRule
{
    List<string> Check(AccessBadgeSchedule item);
}

public sealed record AccessBadgeScheduleRequiredTitleRule(string Message) : IAccessBadgeScheduleRule
{
    public List<string> Check(AccessBadgeSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeScheduleSequenceRangeRule(int Min, int Max) : IAccessBadgeScheduleRule
{
    public List<string> Check(AccessBadgeSchedule item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeScheduleTitleLengthRule(int MaxLength) : IAccessBadgeScheduleRule
{
    public List<string> Check(AccessBadgeSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeScheduleCodeLengthRule(int MaxLength) : IAccessBadgeScheduleRule
{
    public List<string> Check(AccessBadgeSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge schedule.</summary>
public sealed record AccessBadgeScheduleValidator(List<IAccessBadgeScheduleRule> Rules)
{
    public List<string> Validate(AccessBadgeSchedule item)
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

    public void RequireValid(AccessBadgeSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeSchedule> items)
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

/// <summary>Functions of access badge schedule rules.</summary>
public static class AccessBadgeScheduleRules
{
    public static AccessBadgeScheduleValidator DefaultAccessBadgeScheduleValidator()
    {
        return new AccessBadgeScheduleValidator(
            new List<IAccessBadgeScheduleRule>
            {
                new AccessBadgeScheduleRequiredTitleRule("access badge schedule needs this field"),
                new AccessBadgeScheduleSequenceRangeRule(120, 1000),
                new AccessBadgeScheduleTitleLengthRule(1000),
                new AccessBadgeScheduleCodeLengthRule(80),
            });
    }
}
