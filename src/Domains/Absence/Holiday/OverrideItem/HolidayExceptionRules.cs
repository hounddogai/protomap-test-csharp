namespace Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>A rule a holiday exception must pass. Check returns its problems, or none.</summary>
public interface IHolidayExceptionRule
{
    List<string> Check(HolidayException item);
}

public sealed record HolidayExceptionFteRangeRule(double Min, double Max) : IHolidayExceptionRule
{
    public List<string> Check(HolidayException item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HolidayExceptionTitleLengthRule(int MaxLength) : IHolidayExceptionRule
{
    public List<string> Check(HolidayException item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayExceptionRatingRangeRule(double Min, double Max) : IHolidayExceptionRule
{
    public List<string> Check(HolidayException item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HolidayExceptionRequiredEmployeeIdRule(string Message) : IHolidayExceptionRule
{
    public List<string> Check(HolidayException item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday exception.</summary>
public sealed record HolidayExceptionValidator(List<IHolidayExceptionRule> Rules)
{
    public List<string> Validate(HolidayException item)
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

    public void RequireValid(HolidayException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayException> items)
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

/// <summary>Functions of holiday exception rules.</summary>
public static class HolidayExceptionRules
{
    public static HolidayExceptionValidator DefaultHolidayExceptionValidator()
    {
        return new HolidayExceptionValidator(
            new List<IHolidayExceptionRule>
            {
                new HolidayExceptionFteRangeRule(3.8, 4364.5),
                new HolidayExceptionTitleLengthRule(255),
                new HolidayExceptionRatingRangeRule(5.0, 4730.5),
                new HolidayExceptionRequiredEmployeeIdRule("holiday exception needs this field"),
            });
    }
}
