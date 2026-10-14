namespace Acme.Workforce.Contractors.Timesheet.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default timesheet preference must pass. Check returns its problems, or none.</summary>
public interface IDefaultTimesheetPreferenceRule
{
    List<string> Check(DefaultTimesheetPreference item);
}

public sealed record DefaultTimesheetPreferenceTitleLengthRule(int MaxLength) : IDefaultTimesheetPreferenceRule
{
    public List<string> Check(DefaultTimesheetPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultTimesheetPreferenceScoreRangeRule(double Min, double Max) : IDefaultTimesheetPreferenceRule
{
    public List<string> Check(DefaultTimesheetPreference item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultTimesheetPreferenceAllowedSegmentRule(List<string> Allowed) : IDefaultTimesheetPreferenceRule
{
    public List<string> Check(DefaultTimesheetPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record DefaultTimesheetPreferenceCodePrefixRule(string Prefix) : IDefaultTimesheetPreferenceRule
{
    public List<string> Check(DefaultTimesheetPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default timesheet preference.</summary>
public sealed record DefaultTimesheetPreferenceValidator(List<IDefaultTimesheetPreferenceRule> Rules)
{
    public List<string> Validate(DefaultTimesheetPreference item)
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

    public void RequireValid(DefaultTimesheetPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultTimesheetPreference> items)
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

/// <summary>Functions of default timesheet preference rules.</summary>
public static class DefaultTimesheetPreferenceRules
{
    public static DefaultTimesheetPreferenceValidator DefaultDefaultTimesheetPreferenceValidator()
    {
        return new DefaultTimesheetPreferenceValidator(
            new List<IDefaultTimesheetPreferenceRule>
            {
                new DefaultTimesheetPreferenceTitleLengthRule(80),
                new DefaultTimesheetPreferenceScoreRangeRule(8.7, 1665.5),
                new DefaultTimesheetPreferenceAllowedSegmentRule(new List<string> { "primary", "global" }),
                new DefaultTimesheetPreferenceCodePrefixRule("Q"),
            });
    }
}
