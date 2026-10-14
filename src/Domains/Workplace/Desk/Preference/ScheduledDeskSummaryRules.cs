namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled desk summary must pass. Check returns its problems, or none.</summary>
public interface IScheduledDeskSummaryRule
{
    List<string> Check(ScheduledDeskSummary item);
}

public sealed record ScheduledDeskSummaryCodeLengthRule(int MaxLength) : IScheduledDeskSummaryRule
{
    public List<string> Check(ScheduledDeskSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledDeskSummaryNotesLengthRule(int MaxLength) : IScheduledDeskSummaryRule
{
    public List<string> Check(ScheduledDeskSummary item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledDeskSummaryAllowedChecksumRule(List<string> Allowed) : IScheduledDeskSummaryRule
{
    public List<string> Check(ScheduledDeskSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledDeskSummaryRequiredTitleRule(string Message) : IScheduledDeskSummaryRule
{
    public List<string> Check(ScheduledDeskSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledDeskSummaryRateRangeRule(double Min, double Max) : IScheduledDeskSummaryRule
{
    public List<string> Check(ScheduledDeskSummary item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled desk summary.</summary>
public sealed record ScheduledDeskSummaryValidator(List<IScheduledDeskSummaryRule> Rules)
{
    public List<string> Validate(ScheduledDeskSummary item)
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

    public void RequireValid(ScheduledDeskSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledDeskSummary> items)
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

/// <summary>Functions of scheduled desk summary rules.</summary>
public static class ScheduledDeskSummaryRules
{
    public static ScheduledDeskSummaryValidator DefaultScheduledDeskSummaryValidator()
    {
        return new ScheduledDeskSummaryValidator(
            new List<IScheduledDeskSummaryRule>
            {
                new ScheduledDeskSummaryCodeLengthRule(16),
                new ScheduledDeskSummaryNotesLengthRule(86400),
                new ScheduledDeskSummaryAllowedChecksumRule(new List<string> { "fallback" }),
                new ScheduledDeskSummaryRequiredTitleRule("scheduled desk summary needs a value"),
                new ScheduledDeskSummaryRateRangeRule(5.7, 4825.8),
            });
    }
}
