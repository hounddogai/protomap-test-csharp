namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>A rule a regional timesheet milestone must pass. Check returns its problems, or none.</summary>
public interface IRegionalTimesheetMilestoneRule
{
    List<string> Check(RegionalTimesheetMilestone item);
}

public sealed record RegionalTimesheetMilestoneRequiredCodeRule(string Message) : IRegionalTimesheetMilestoneRule
{
    public List<string> Check(RegionalTimesheetMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetMilestoneCostCenterLengthRule(int MaxLength) : IRegionalTimesheetMilestoneRule
{
    public List<string> Check(RegionalTimesheetMilestone item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetMilestoneScoreRangeRule(double Min, double Max) : IRegionalTimesheetMilestoneRule
{
    public List<string> Check(RegionalTimesheetMilestone item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetMilestoneCostRangeRule(double Min, double Max) : IRegionalTimesheetMilestoneRule
{
    public List<string> Check(RegionalTimesheetMilestone item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalTimesheetMilestoneCeilingRangeRule(double Min, double Max) : IRegionalTimesheetMilestoneRule
{
    public List<string> Check(RegionalTimesheetMilestone item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional timesheet milestone.</summary>
public sealed record RegionalTimesheetMilestoneValidator(List<IRegionalTimesheetMilestoneRule> Rules)
{
    public List<string> Validate(RegionalTimesheetMilestone item)
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

    public void RequireValid(RegionalTimesheetMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalTimesheetMilestone> items)
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

/// <summary>Functions of regional timesheet milestone rules.</summary>
public static class RegionalTimesheetMilestoneRules
{
    public static RegionalTimesheetMilestoneValidator DefaultRegionalTimesheetMilestoneValidator()
    {
        return new RegionalTimesheetMilestoneValidator(
            new List<IRegionalTimesheetMilestoneRule>
            {
                new RegionalTimesheetMilestoneRequiredCodeRule("regional timesheet milestone needs a value"),
                new RegionalTimesheetMilestoneCostCenterLengthRule(64),
                new RegionalTimesheetMilestoneScoreRangeRule(7.7, 2295.6),
                new RegionalTimesheetMilestoneCostRangeRule(3.4, 4622.6),
                new RegionalTimesheetMilestoneCeilingRangeRule(9.5, 2702.2),
            });
    }
}
