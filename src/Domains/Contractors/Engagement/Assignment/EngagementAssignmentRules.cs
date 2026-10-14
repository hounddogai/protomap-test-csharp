namespace Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>A rule a engagement assignment must pass. Check returns its problems, or none.</summary>
public interface IEngagementAssignmentRule
{
    List<string> Check(EngagementAssignment item);
}

public sealed record EngagementAssignmentRequiredSourceRule(string Message) : IEngagementAssignmentRule
{
    public List<string> Check(EngagementAssignment item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementAssignmentDurationMinutesRangeRule(int Min, int Max) : IEngagementAssignmentRule
{
    public List<string> Check(EngagementAssignment item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementAssignmentCodeLengthRule(int MaxLength) : IEngagementAssignmentRule
{
    public List<string> Check(EngagementAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementAssignmentSourceLengthRule(int MaxLength) : IEngagementAssignmentRule
{
    public List<string> Check(EngagementAssignment item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementAssignmentRequiredNameRule(string Message) : IEngagementAssignmentRule
{
    public List<string> Check(EngagementAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementAssignmentStartsAtRangeRule(int Min, int Max) : IEngagementAssignmentRule
{
    public List<string> Check(EngagementAssignment item)
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

/// <summary>Runs every rule of a engagement assignment.</summary>
public sealed record EngagementAssignmentValidator(List<IEngagementAssignmentRule> Rules)
{
    public List<string> Validate(EngagementAssignment item)
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

    public void RequireValid(EngagementAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementAssignment> items)
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

/// <summary>Functions of engagement assignment rules.</summary>
public static class EngagementAssignmentRules
{
    public static EngagementAssignmentValidator DefaultEngagementAssignmentValidator()
    {
        return new EngagementAssignmentValidator(
            new List<IEngagementAssignmentRule>
            {
                new EngagementAssignmentRequiredSourceRule("engagement assignment needs a value"),
                new EngagementAssignmentDurationMinutesRangeRule(64, 120),
                new EngagementAssignmentCodeLengthRule(3600),
                new EngagementAssignmentSourceLengthRule(86400),
                new EngagementAssignmentRequiredNameRule("engagement assignment needs a value"),
                new EngagementAssignmentStartsAtRangeRule(120, 86400),
            });
    }
}
