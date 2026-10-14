namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

/// <summary>A rule a legacy holiday schedule must pass. Check returns its problems, or none.</summary>
public interface ILegacyHolidayScheduleRule
{
    List<string> Check(LegacyHolidaySchedule item);
}

public sealed record LegacyHolidayScheduleRequiredJobCodeRule(string Message) : ILegacyHolidayScheduleRule
{
    public List<string> Check(LegacyHolidaySchedule item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyHolidayScheduleJobCodeLengthRule(int MaxLength) : ILegacyHolidayScheduleRule
{
    public List<string> Check(LegacyHolidaySchedule item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyHolidayScheduleRatingRangeRule(double Min, double Max) : ILegacyHolidayScheduleRule
{
    public List<string> Check(LegacyHolidaySchedule item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyHolidayScheduleLabelLengthRule(int MaxLength) : ILegacyHolidayScheduleRule
{
    public List<string> Check(LegacyHolidaySchedule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy holiday schedule.</summary>
public sealed record LegacyHolidayScheduleValidator(List<ILegacyHolidayScheduleRule> Rules)
{
    public List<string> Validate(LegacyHolidaySchedule item)
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

    public void RequireValid(LegacyHolidaySchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyHolidaySchedule> items)
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

/// <summary>Functions of legacy holiday schedule rules.</summary>
public static class LegacyHolidayScheduleRules
{
    public static LegacyHolidayScheduleValidator DefaultLegacyHolidayScheduleValidator()
    {
        return new LegacyHolidayScheduleValidator(
            new List<ILegacyHolidayScheduleRule>
            {
                new LegacyHolidayScheduleRequiredJobCodeRule("legacy holiday schedule needs an entry"),
                new LegacyHolidayScheduleJobCodeLengthRule(1000),
                new LegacyHolidayScheduleRatingRangeRule(9.3, 1601.9),
                new LegacyHolidayScheduleLabelLengthRule(3600),
            });
    }
}
