namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled timesheet target must pass. Check returns its problems, or none.</summary>
public interface IScheduledTimesheetTargetRule
{
    List<string> Check(ScheduledTimesheetTarget item);
}

public sealed record ScheduledTimesheetTargetTenureMonthsRangeRule(int Min, int Max) : IScheduledTimesheetTargetRule
{
    public List<string> Check(ScheduledTimesheetTarget item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTimesheetTargetTitleLengthRule(int MaxLength) : IScheduledTimesheetTargetRule
{
    public List<string> Check(ScheduledTimesheetTarget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTimesheetTargetRequiredTitleRule(string Message) : IScheduledTimesheetTargetRule
{
    public List<string> Check(ScheduledTimesheetTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledTimesheetTargetAllowedTitleRule(List<string> Allowed) : IScheduledTimesheetTargetRule
{
    public List<string> Check(ScheduledTimesheetTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled timesheet target.</summary>
public sealed record ScheduledTimesheetTargetValidator(List<IScheduledTimesheetTargetRule> Rules)
{
    public List<string> Validate(ScheduledTimesheetTarget item)
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

    public void RequireValid(ScheduledTimesheetTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledTimesheetTarget> items)
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

/// <summary>Functions of scheduled timesheet target rules.</summary>
public static class ScheduledTimesheetTargetRules
{
    public static ScheduledTimesheetTargetValidator DefaultScheduledTimesheetTargetValidator()
    {
        return new ScheduledTimesheetTargetValidator(
            new List<IScheduledTimesheetTargetRule>
            {
                new ScheduledTimesheetTargetTenureMonthsRangeRule(16, 86400),
                new ScheduledTimesheetTargetTitleLengthRule(16),
                new ScheduledTimesheetTargetRequiredTitleRule("scheduled timesheet target needs a value"),
                new ScheduledTimesheetTargetAllowedTitleRule(new List<string> { "adaptive", "pending" }),
            });
    }
}
