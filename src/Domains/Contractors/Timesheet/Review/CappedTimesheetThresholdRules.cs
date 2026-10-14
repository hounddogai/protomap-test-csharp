namespace Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>A rule a capped timesheet threshold must pass. Check returns its problems, or none.</summary>
public interface ICappedTimesheetThresholdRule
{
    List<string> Check(CappedTimesheetThreshold item);
}

public sealed record CappedTimesheetThresholdNameLengthRule(int MaxLength) : ICappedTimesheetThresholdRule
{
    public List<string> Check(CappedTimesheetThreshold item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTimesheetThresholdMaximumRangeRule(double Min, double Max) : ICappedTimesheetThresholdRule
{
    public List<string> Check(CappedTimesheetThreshold item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedTimesheetThresholdThresholdRangeRule(double Min, double Max) : ICappedTimesheetThresholdRule
{
    public List<string> Check(CappedTimesheetThreshold item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedTimesheetThresholdCeilingRangeRule(double Min, double Max) : ICappedTimesheetThresholdRule
{
    public List<string> Check(CappedTimesheetThreshold item)
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

public sealed record CappedTimesheetThresholdRequiredDescriptionRule(string Message) : ICappedTimesheetThresholdRule
{
    public List<string> Check(CappedTimesheetThreshold item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped timesheet threshold.</summary>
public sealed record CappedTimesheetThresholdValidator(List<ICappedTimesheetThresholdRule> Rules)
{
    public List<string> Validate(CappedTimesheetThreshold item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(CappedTimesheetThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedTimesheetThreshold> items)
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

/// <summary>Functions of capped timesheet threshold rules.</summary>
public static class CappedTimesheetThresholdRules
{
    public static CappedTimesheetThresholdValidator DefaultCappedTimesheetThresholdValidator()
    {
        return new CappedTimesheetThresholdValidator(
            new List<ICappedTimesheetThresholdRule>
            {
                new CappedTimesheetThresholdNameLengthRule(64),
                new CappedTimesheetThresholdMaximumRangeRule(0.6, 754.0),
                new CappedTimesheetThresholdThresholdRangeRule(7.3, 1553.8),
                new CappedTimesheetThresholdCeilingRangeRule(3.6, 2634.3),
                new CappedTimesheetThresholdRequiredDescriptionRule("capped timesheet threshold needs this field"),
            });
    }
}
