namespace Acme.Workforce.Absence.Holiday.Tier;

/// <summary>A rule a holiday tier must pass. Check returns its problems, or none.</summary>
public interface IHolidayTierRule
{
    List<string> Check(HolidayTier item);
}

public sealed record HolidayTierGraceDaysRangeRule(int Min, int Max) : IHolidayTierRule
{
    public List<string> Check(HolidayTier item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayTierCodeLengthRule(int MaxLength) : IHolidayTierRule
{
    public List<string> Check(HolidayTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayTierTitleLengthRule(int MaxLength) : IHolidayTierRule
{
    public List<string> Check(HolidayTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayTierEmployeeIdLengthRule(int MaxLength) : IHolidayTierRule
{
    public List<string> Check(HolidayTier item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayTierRatingRangeRule(double Min, double Max) : IHolidayTierRule
{
    public List<string> Check(HolidayTier item)
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

/// <summary>Runs every rule of a holiday tier.</summary>
public sealed record HolidayTierValidator(List<IHolidayTierRule> Rules)
{
    public List<string> Validate(HolidayTier item)
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

    public void RequireValid(HolidayTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayTier> items)
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

/// <summary>Functions of holiday tier rules.</summary>
public static class HolidayTierRules
{
    public static HolidayTierValidator DefaultHolidayTierValidator()
    {
        return new HolidayTierValidator(
            new List<IHolidayTierRule>
            {
                new HolidayTierGraceDaysRangeRule(3600, 86400),
                new HolidayTierCodeLengthRule(1000),
                new HolidayTierTitleLengthRule(80),
                new HolidayTierEmployeeIdLengthRule(1000),
                new HolidayTierRatingRangeRule(4.0, 3561.5),
            });
    }
}
