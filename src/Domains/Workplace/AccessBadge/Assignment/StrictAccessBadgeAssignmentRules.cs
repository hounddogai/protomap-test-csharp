namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

/// <summary>A rule a strict access badge assignment must pass. Check returns its problems, or none.</summary>
public interface IStrictAccessBadgeAssignmentRule
{
    List<string> Check(StrictAccessBadgeAssignment item);
}

public sealed record StrictAccessBadgeAssignmentTeamLengthRule(int MaxLength) : IStrictAccessBadgeAssignmentRule
{
    public List<string> Check(StrictAccessBadgeAssignment item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccessBadgeAssignmentCodePrefixRule(string Prefix) : IStrictAccessBadgeAssignmentRule
{
    public List<string> Check(StrictAccessBadgeAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAccessBadgeAssignmentUpdatedAtRangeRule(int Min, int Max) : IStrictAccessBadgeAssignmentRule
{
    public List<string> Check(StrictAccessBadgeAssignment item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictAccessBadgeAssignmentRequiredSkillRule(string Message) : IStrictAccessBadgeAssignmentRule
{
    public List<string> Check(StrictAccessBadgeAssignment item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict access badge assignment.</summary>
public sealed record StrictAccessBadgeAssignmentValidator(List<IStrictAccessBadgeAssignmentRule> Rules)
{
    public List<string> Validate(StrictAccessBadgeAssignment item)
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

    public void RequireValid(StrictAccessBadgeAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAccessBadgeAssignment> items)
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

/// <summary>Functions of strict access badge assignment rules.</summary>
public static class StrictAccessBadgeAssignmentRules
{
    public static StrictAccessBadgeAssignmentValidator DefaultStrictAccessBadgeAssignmentValidator()
    {
        return new StrictAccessBadgeAssignmentValidator(
            new List<IStrictAccessBadgeAssignmentRule>
            {
                new StrictAccessBadgeAssignmentTeamLengthRule(80),
                new StrictAccessBadgeAssignmentCodePrefixRule("X"),
                new StrictAccessBadgeAssignmentUpdatedAtRangeRule(3600, 86400),
                new StrictAccessBadgeAssignmentRequiredSkillRule("strict access badge assignment needs a value"),
            });
    }
}
