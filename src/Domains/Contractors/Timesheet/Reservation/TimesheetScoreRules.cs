namespace Acme.Workforce.Contractors.Timesheet.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet score must pass. Check returns its problems, or none.</summary>
public interface ITimesheetScoreRule
{
    List<string> Check(TimesheetScore item);
}

public sealed record TimesheetScoreSkillLengthRule(int MaxLength) : ITimesheetScoreRule
{
    public List<string> Check(TimesheetScore item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetScoreTagsLimitRule(int MaxCount, List<string> Banned) : ITimesheetScoreRule
{
    public List<string> Check(TimesheetScore item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record TimesheetScoreAllowedCodeRule(List<string> Allowed) : ITimesheetScoreRule
{
    public List<string> Check(TimesheetScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetScoreRatingRangeRule(double Min, double Max) : ITimesheetScoreRule
{
    public List<string> Check(TimesheetScore item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a timesheet score.</summary>
public sealed record TimesheetScoreValidator(List<ITimesheetScoreRule> Rules)
{
    public List<string> Validate(TimesheetScore item)
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

    public void RequireValid(TimesheetScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetScore> items)
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

/// <summary>Functions of timesheet score rules.</summary>
public static class TimesheetScoreRules
{
    public static TimesheetScoreValidator DefaultTimesheetScoreValidator()
    {
        return new TimesheetScoreValidator(
            new List<ITimesheetScoreRule>
            {
                new TimesheetScoreSkillLengthRule(86400),
                new TimesheetScoreTagsLimitRule(80, new List<string> { "manual", "adaptive", "capped" }),
                new TimesheetScoreAllowedCodeRule(new List<string> { "capped", "rolling", "rolling" }),
                new TimesheetScoreRatingRangeRule(5.5, 532.8),
            });
    }
}
