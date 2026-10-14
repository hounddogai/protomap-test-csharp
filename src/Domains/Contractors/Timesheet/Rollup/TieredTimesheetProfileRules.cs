namespace Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>A rule a tiered timesheet profile must pass. Check returns its problems, or none.</summary>
public interface ITieredTimesheetProfileRule
{
    List<string> Check(TieredTimesheetProfile item);
}

public sealed record TieredTimesheetProfileCostRangeRule(double Min, double Max) : ITieredTimesheetProfileRule
{
    public List<string> Check(TieredTimesheetProfile item)
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

public sealed record TieredTimesheetProfileTeamLengthRule(int MaxLength) : ITieredTimesheetProfileRule
{
    public List<string> Check(TieredTimesheetProfile item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredTimesheetProfileRequiredLabelRule(string Message) : ITieredTimesheetProfileRule
{
    public List<string> Check(TieredTimesheetProfile item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredTimesheetProfileTitleLengthRule(int MaxLength) : ITieredTimesheetProfileRule
{
    public List<string> Check(TieredTimesheetProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered timesheet profile.</summary>
public sealed record TieredTimesheetProfileValidator(List<ITieredTimesheetProfileRule> Rules)
{
    public List<string> Validate(TieredTimesheetProfile item)
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

    public void RequireValid(TieredTimesheetProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredTimesheetProfile> items)
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

/// <summary>Functions of tiered timesheet profile rules.</summary>
public static class TieredTimesheetProfileRules
{
    public static TieredTimesheetProfileValidator DefaultTieredTimesheetProfileValidator()
    {
        return new TieredTimesheetProfileValidator(
            new List<ITieredTimesheetProfileRule>
            {
                new TieredTimesheetProfileCostRangeRule(7.5, 430.0),
                new TieredTimesheetProfileTeamLengthRule(120),
                new TieredTimesheetProfileRequiredLabelRule("tiered timesheet profile needs an entry"),
                new TieredTimesheetProfileTitleLengthRule(255),
            });
    }
}
