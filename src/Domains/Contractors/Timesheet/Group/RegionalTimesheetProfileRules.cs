namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional timesheet profile must pass. Check returns its problems, or none.</summary>
public interface IRegionalTimesheetProfileRule
{
    List<string> Check(RegionalTimesheetProfile item);
}

public sealed record RegionalTimesheetProfileTimezoneLengthRule(int MaxLength) : IRegionalTimesheetProfileRule
{
    public List<string> Check(RegionalTimesheetProfile item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetProfileBatchSizeRangeRule(int Min, int Max) : IRegionalTimesheetProfileRule
{
    public List<string> Check(RegionalTimesheetProfile item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalTimesheetProfileRequiredNameRule(string Message) : IRegionalTimesheetProfileRule
{
    public List<string> Check(RegionalTimesheetProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetProfileRequiredCodeRule(string Message) : IRegionalTimesheetProfileRule
{
    public List<string> Check(RegionalTimesheetProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetProfileAllowedGradeRule(List<string> Allowed) : IRegionalTimesheetProfileRule
{
    public List<string> Check(RegionalTimesheetProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record RegionalTimesheetProfileRequiredTimezoneRule(string Message) : IRegionalTimesheetProfileRule
{
    public List<string> Check(RegionalTimesheetProfile item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional timesheet profile.</summary>
public sealed record RegionalTimesheetProfileValidator(List<IRegionalTimesheetProfileRule> Rules)
{
    public List<string> Validate(RegionalTimesheetProfile item)
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

    public void RequireValid(RegionalTimesheetProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalTimesheetProfile> items)
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

/// <summary>Functions of regional timesheet profile rules.</summary>
public static class RegionalTimesheetProfileRules
{
    public static RegionalTimesheetProfileValidator DefaultRegionalTimesheetProfileValidator()
    {
        return new RegionalTimesheetProfileValidator(
            new List<IRegionalTimesheetProfileRule>
            {
                new RegionalTimesheetProfileTimezoneLengthRule(1000),
                new RegionalTimesheetProfileBatchSizeRangeRule(86400, 86400),
                new RegionalTimesheetProfileRequiredNameRule("regional timesheet profile needs an entry"),
                new RegionalTimesheetProfileRequiredCodeRule("regional timesheet profile needs this field"),
                new RegionalTimesheetProfileAllowedGradeRule(new List<string> { "pending", "rolling" }),
                new RegionalTimesheetProfileRequiredTimezoneRule("regional timesheet profile needs this field"),
            });
    }
}
