namespace Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>A rule a manual holiday variance must pass. Check returns its problems, or none.</summary>
public interface IManualHolidayVarianceRule
{
    List<string> Check(ManualHolidayVariance item);
}

public sealed record ManualHolidayVarianceMinimumRangeRule(double Min, double Max) : IManualHolidayVarianceRule
{
    public List<string> Check(ManualHolidayVariance item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualHolidayVarianceTitleLengthRule(int MaxLength) : IManualHolidayVarianceRule
{
    public List<string> Check(ManualHolidayVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualHolidayVarianceHoursRangeRule(double Min, double Max) : IManualHolidayVarianceRule
{
    public List<string> Check(ManualHolidayVariance item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualHolidayVarianceRequiredJobCodeRule(string Message) : IManualHolidayVarianceRule
{
    public List<string> Check(ManualHolidayVariance item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual holiday variance.</summary>
public sealed record ManualHolidayVarianceValidator(List<IManualHolidayVarianceRule> Rules)
{
    public List<string> Validate(ManualHolidayVariance item)
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

    public void RequireValid(ManualHolidayVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualHolidayVariance> items)
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

/// <summary>Functions of manual holiday variance rules.</summary>
public static class ManualHolidayVarianceRules
{
    public static ManualHolidayVarianceValidator DefaultManualHolidayVarianceValidator()
    {
        return new ManualHolidayVarianceValidator(
            new List<IManualHolidayVarianceRule>
            {
                new ManualHolidayVarianceMinimumRangeRule(9.5, 1125.2),
                new ManualHolidayVarianceTitleLengthRule(120),
                new ManualHolidayVarianceHoursRangeRule(4.5, 207.3),
                new ManualHolidayVarianceRequiredJobCodeRule("manual holiday variance needs an entry"),
            });
    }
}
