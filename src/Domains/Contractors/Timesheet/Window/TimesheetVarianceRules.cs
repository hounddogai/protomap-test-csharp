namespace Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>A rule a timesheet variance must pass. Check returns its problems, or none.</summary>
public interface ITimesheetVarianceRule
{
    List<string> Check(TimesheetVariance item);
}

public sealed record TimesheetVarianceScoreRangeRule(double Min, double Max) : ITimesheetVarianceRule
{
    public List<string> Check(TimesheetVariance item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetVarianceRateRangeRule(double Min, double Max) : ITimesheetVarianceRule
{
    public List<string> Check(TimesheetVariance item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetVarianceReviewerLengthRule(int MaxLength) : ITimesheetVarianceRule
{
    public List<string> Check(TimesheetVariance item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetVarianceRequiredReviewerRule(string Message) : ITimesheetVarianceRule
{
    public List<string> Check(TimesheetVariance item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a timesheet variance.</summary>
public sealed record TimesheetVarianceValidator(List<ITimesheetVarianceRule> Rules)
{
    public List<string> Validate(TimesheetVariance item)
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

    public void RequireValid(TimesheetVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetVariance> items)
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

/// <summary>Functions of timesheet variance rules.</summary>
public static class TimesheetVarianceRules
{
    public static TimesheetVarianceValidator DefaultTimesheetVarianceValidator()
    {
        return new TimesheetVarianceValidator(
            new List<ITimesheetVarianceRule>
            {
                new TimesheetVarianceScoreRangeRule(6.4, 1688.8),
                new TimesheetVarianceRateRangeRule(3.0, 2144.7),
                new TimesheetVarianceReviewerLengthRule(255),
                new TimesheetVarianceRequiredReviewerRule("timesheet variance needs an entry"),
            });
    }
}
