namespace Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>A rule a seasonal holiday exception must pass. Check returns its problems, or none.</summary>
public interface ISeasonalHolidayExceptionRule
{
    List<string> Check(SeasonalHolidayException item);
}

public sealed record SeasonalHolidayExceptionRetriesRangeRule(int Min, int Max) : ISeasonalHolidayExceptionRule
{
    public List<string> Check(SeasonalHolidayException item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalHolidayExceptionFloorValueRangeRule(double Min, double Max) : ISeasonalHolidayExceptionRule
{
    public List<string> Check(SeasonalHolidayException item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalHolidayExceptionNameLengthRule(int MaxLength) : ISeasonalHolidayExceptionRule
{
    public List<string> Check(SeasonalHolidayException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal holiday exception.</summary>
public sealed record SeasonalHolidayExceptionValidator(List<ISeasonalHolidayExceptionRule> Rules)
{
    public List<string> Validate(SeasonalHolidayException item)
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

    public void RequireValid(SeasonalHolidayException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalHolidayException> items)
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

/// <summary>Functions of seasonal holiday exception rules.</summary>
public static class SeasonalHolidayExceptionRules
{
    public static SeasonalHolidayExceptionValidator DefaultSeasonalHolidayExceptionValidator()
    {
        return new SeasonalHolidayExceptionValidator(
            new List<ISeasonalHolidayExceptionRule>
            {
                new SeasonalHolidayExceptionRetriesRangeRule(32, 32),
                new SeasonalHolidayExceptionFloorValueRangeRule(0.9, 1303.6),
                new SeasonalHolidayExceptionNameLengthRule(1000),
            });
    }
}
