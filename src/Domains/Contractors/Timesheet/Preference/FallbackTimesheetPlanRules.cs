namespace Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>A rule a fallback timesheet plan must pass. Check returns its problems, or none.</summary>
public interface IFallbackTimesheetPlanRule
{
    List<string> Check(FallbackTimesheetPlan item);
}

public sealed record FallbackTimesheetPlanFteRangeRule(double Min, double Max) : IFallbackTimesheetPlanRule
{
    public List<string> Check(FallbackTimesheetPlan item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTimesheetPlanCodeLengthRule(int MaxLength) : IFallbackTimesheetPlanRule
{
    public List<string> Check(FallbackTimesheetPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTimesheetPlanSummaryLengthRule(int MaxLength) : IFallbackTimesheetPlanRule
{
    public List<string> Check(FallbackTimesheetPlan item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTimesheetPlanNotesLengthRule(int MaxLength) : IFallbackTimesheetPlanRule
{
    public List<string> Check(FallbackTimesheetPlan item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackTimesheetPlanMarginRangeRule(double Min, double Max) : IFallbackTimesheetPlanRule
{
    public List<string> Check(FallbackTimesheetPlan item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback timesheet plan.</summary>
public sealed record FallbackTimesheetPlanValidator(List<IFallbackTimesheetPlanRule> Rules)
{
    public List<string> Validate(FallbackTimesheetPlan item)
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

    public void RequireValid(FallbackTimesheetPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackTimesheetPlan> items)
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

/// <summary>Functions of fallback timesheet plan rules.</summary>
public static class FallbackTimesheetPlanRules
{
    public static FallbackTimesheetPlanValidator DefaultFallbackTimesheetPlanValidator()
    {
        return new FallbackTimesheetPlanValidator(
            new List<IFallbackTimesheetPlanRule>
            {
                new FallbackTimesheetPlanFteRangeRule(2.1, 1806.5),
                new FallbackTimesheetPlanCodeLengthRule(80),
                new FallbackTimesheetPlanSummaryLengthRule(255),
                new FallbackTimesheetPlanNotesLengthRule(1000),
                new FallbackTimesheetPlanMarginRangeRule(5.2, 2961.4),
            });
    }
}
