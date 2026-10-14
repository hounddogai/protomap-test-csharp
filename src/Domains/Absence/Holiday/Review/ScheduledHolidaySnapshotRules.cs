namespace Acme.Workforce.Absence.Holiday.Review;

/// <summary>A rule a scheduled holiday snapshot must pass. Check returns its problems, or none.</summary>
public interface IScheduledHolidaySnapshotRule
{
    List<string> Check(ScheduledHolidaySnapshot item);
}

public sealed record ScheduledHolidaySnapshotCodeLengthRule(int MaxLength) : IScheduledHolidaySnapshotRule
{
    public List<string> Check(ScheduledHolidaySnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledHolidaySnapshotHoursRangeRule(double Min, double Max) : IScheduledHolidaySnapshotRule
{
    public List<string> Check(ScheduledHolidaySnapshot item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledHolidaySnapshotLabelLengthRule(int MaxLength) : IScheduledHolidaySnapshotRule
{
    public List<string> Check(ScheduledHolidaySnapshot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled holiday snapshot.</summary>
public sealed record ScheduledHolidaySnapshotValidator(List<IScheduledHolidaySnapshotRule> Rules)
{
    public List<string> Validate(ScheduledHolidaySnapshot item)
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

    public void RequireValid(ScheduledHolidaySnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledHolidaySnapshot> items)
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

/// <summary>Functions of scheduled holiday snapshot rules.</summary>
public static class ScheduledHolidaySnapshotRules
{
    public static ScheduledHolidaySnapshotValidator DefaultScheduledHolidaySnapshotValidator()
    {
        return new ScheduledHolidaySnapshotValidator(
            new List<IScheduledHolidaySnapshotRule>
            {
                new ScheduledHolidaySnapshotCodeLengthRule(16),
                new ScheduledHolidaySnapshotHoursRangeRule(0.1, 4338.0),
                new ScheduledHolidaySnapshotLabelLengthRule(32),
            });
    }
}
