namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>A rule a lenient timesheet milestone must pass. Check returns its problems, or none.</summary>
public interface ILenientTimesheetMilestoneRule
{
    List<string> Check(LenientTimesheetMilestone item);
}

public sealed record LenientTimesheetMilestoneCodePrefixRule(string Prefix) : ILenientTimesheetMilestoneRule
{
    public List<string> Check(LenientTimesheetMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientTimesheetMilestoneCodeLengthRule(int MaxLength) : ILenientTimesheetMilestoneRule
{
    public List<string> Check(LenientTimesheetMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientTimesheetMilestoneRequiredCodeRule(string Message) : ILenientTimesheetMilestoneRule
{
    public List<string> Check(LenientTimesheetMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient timesheet milestone.</summary>
public sealed record LenientTimesheetMilestoneValidator(List<ILenientTimesheetMilestoneRule> Rules)
{
    public List<string> Validate(LenientTimesheetMilestone item)
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

    public void RequireValid(LenientTimesheetMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientTimesheetMilestone> items)
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

/// <summary>Functions of lenient timesheet milestone rules.</summary>
public static class LenientTimesheetMilestoneRules
{
    public static LenientTimesheetMilestoneValidator DefaultLenientTimesheetMilestoneValidator()
    {
        return new LenientTimesheetMilestoneValidator(
            new List<ILenientTimesheetMilestoneRule>
            {
                new LenientTimesheetMilestoneCodePrefixRule("ACM"),
                new LenientTimesheetMilestoneCodeLengthRule(120),
                new LenientTimesheetMilestoneRequiredCodeRule("lenient timesheet milestone needs an entry"),
            });
    }
}
