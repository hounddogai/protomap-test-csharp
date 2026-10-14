namespace Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>A rule a lenient timesheet plan must pass. Check returns its problems, or none.</summary>
public interface ILenientTimesheetPlanRule
{
    List<string> Check(LenientTimesheetPlan item);
}

public sealed record LenientTimesheetPlanRequiredNameRule(string Message) : ILenientTimesheetPlanRule
{
    public List<string> Check(LenientTimesheetPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientTimesheetPlanRequiredSegmentRule(string Message) : ILenientTimesheetPlanRule
{
    public List<string> Check(LenientTimesheetPlan item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientTimesheetPlanRequiredLocationRule(string Message) : ILenientTimesheetPlanRule
{
    public List<string> Check(LenientTimesheetPlan item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientTimesheetPlanLocationLengthRule(int MaxLength) : ILenientTimesheetPlanRule
{
    public List<string> Check(LenientTimesheetPlan item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientTimesheetPlanCodePrefixRule(string Prefix) : ILenientTimesheetPlanRule
{
    public List<string> Check(LenientTimesheetPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient timesheet plan.</summary>
public sealed record LenientTimesheetPlanValidator(List<ILenientTimesheetPlanRule> Rules)
{
    public List<string> Validate(LenientTimesheetPlan item)
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

    public void RequireValid(LenientTimesheetPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientTimesheetPlan> items)
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

/// <summary>Functions of lenient timesheet plan rules.</summary>
public static class LenientTimesheetPlanRules
{
    public static LenientTimesheetPlanValidator DefaultLenientTimesheetPlanValidator()
    {
        return new LenientTimesheetPlanValidator(
            new List<ILenientTimesheetPlanRule>
            {
                new LenientTimesheetPlanRequiredNameRule("lenient timesheet plan needs this field"),
                new LenientTimesheetPlanRequiredSegmentRule("lenient timesheet plan needs an entry"),
                new LenientTimesheetPlanRequiredLocationRule("lenient timesheet plan needs a value"),
                new LenientTimesheetPlanLocationLengthRule(120),
                new LenientTimesheetPlanCodePrefixRule("OPS"),
            });
    }
}
