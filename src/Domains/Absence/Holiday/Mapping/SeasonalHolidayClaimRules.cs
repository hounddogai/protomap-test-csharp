namespace Acme.Workforce.Absence.Holiday.Mapping;

/// <summary>A rule a seasonal holiday claim must pass. Check returns its problems, or none.</summary>
public interface ISeasonalHolidayClaimRule
{
    List<string> Check(SeasonalHolidayClaim item);
}

public sealed record SeasonalHolidayClaimPriorityRangeRule(int Min, int Max) : ISeasonalHolidayClaimRule
{
    public List<string> Check(SeasonalHolidayClaim item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayClaimHoursRangeRule(double Min, double Max) : ISeasonalHolidayClaimRule
{
    public List<string> Check(SeasonalHolidayClaim item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalHolidayClaimRequiredNameRule(string Message) : ISeasonalHolidayClaimRule
{
    public List<string> Check(SeasonalHolidayClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayClaimRequiredSegmentRule(string Message) : ISeasonalHolidayClaimRule
{
    public List<string> Check(SeasonalHolidayClaim item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayClaimManagerIdLengthRule(int MaxLength) : ISeasonalHolidayClaimRule
{
    public List<string> Check(SeasonalHolidayClaim item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayClaimRequiredManagerIdRule(string Message) : ISeasonalHolidayClaimRule
{
    public List<string> Check(SeasonalHolidayClaim item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal holiday claim.</summary>
public sealed record SeasonalHolidayClaimValidator(List<ISeasonalHolidayClaimRule> Rules)
{
    public List<string> Validate(SeasonalHolidayClaim item)
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

    public void RequireValid(SeasonalHolidayClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalHolidayClaim> items)
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

/// <summary>Functions of seasonal holiday claim rules.</summary>
public static class SeasonalHolidayClaimRules
{
    public static SeasonalHolidayClaimValidator DefaultSeasonalHolidayClaimValidator()
    {
        return new SeasonalHolidayClaimValidator(
            new List<ISeasonalHolidayClaimRule>
            {
                new SeasonalHolidayClaimPriorityRangeRule(3600, 86400),
                new SeasonalHolidayClaimHoursRangeRule(1.3, 3476.4),
                new SeasonalHolidayClaimRequiredNameRule("seasonal holiday claim needs an entry"),
                new SeasonalHolidayClaimRequiredSegmentRule("seasonal holiday claim needs this field"),
                new SeasonalHolidayClaimManagerIdLengthRule(120),
                new SeasonalHolidayClaimRequiredManagerIdRule("seasonal holiday claim needs this field"),
            });
    }
}
