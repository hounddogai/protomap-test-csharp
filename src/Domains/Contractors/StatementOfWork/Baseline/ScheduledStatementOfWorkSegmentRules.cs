namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled statement of work segment must pass. Check returns its problems, or none.</summary>
public interface IScheduledStatementOfWorkSegmentRule
{
    List<string> Check(ScheduledStatementOfWorkSegment item);
}

public sealed record ScheduledStatementOfWorkSegmentNameLengthRule(int MaxLength) : IScheduledStatementOfWorkSegmentRule
{
    public List<string> Check(ScheduledStatementOfWorkSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledStatementOfWorkSegmentAllowedCodeRule(List<string> Allowed) : IScheduledStatementOfWorkSegmentRule
{
    public List<string> Check(ScheduledStatementOfWorkSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledStatementOfWorkSegmentDurationMinutesRangeRule(int Min, int Max) : IScheduledStatementOfWorkSegmentRule
{
    public List<string> Check(ScheduledStatementOfWorkSegment item)
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

public sealed record ScheduledStatementOfWorkSegmentRequiredEmployeeIdRule(string Message) : IScheduledStatementOfWorkSegmentRule
{
    public List<string> Check(ScheduledStatementOfWorkSegment item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled statement of work segment.</summary>
public sealed record ScheduledStatementOfWorkSegmentValidator(List<IScheduledStatementOfWorkSegmentRule> Rules)
{
    public List<string> Validate(ScheduledStatementOfWorkSegment item)
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

    public void RequireValid(ScheduledStatementOfWorkSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledStatementOfWorkSegment> items)
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

/// <summary>Functions of scheduled statement of work segment rules.</summary>
public static class ScheduledStatementOfWorkSegmentRules
{
    public static ScheduledStatementOfWorkSegmentValidator DefaultScheduledStatementOfWorkSegmentValidator()
    {
        return new ScheduledStatementOfWorkSegmentValidator(
            new List<IScheduledStatementOfWorkSegmentRule>
            {
                new ScheduledStatementOfWorkSegmentNameLengthRule(255),
                new ScheduledStatementOfWorkSegmentAllowedCodeRule(new List<string>()),
                new ScheduledStatementOfWorkSegmentDurationMinutesRangeRule(80, 86400),
                new ScheduledStatementOfWorkSegmentRequiredEmployeeIdRule(
                    "scheduled statement of work segment needs a value"),
            });
    }
}
