namespace Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>A rule a holiday preference must pass. Check returns its problems, or none.</summary>
public interface IHolidayPreferenceRule
{
    List<string> Check(HolidayPreference item);
}

public sealed record HolidayPreferenceRequiredTitleRule(string Message) : IHolidayPreferenceRule
{
    public List<string> Check(HolidayPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayPreferenceRequiredBadgeRule(string Message) : IHolidayPreferenceRule
{
    public List<string> Check(HolidayPreference item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayPreferenceTitleLengthRule(int MaxLength) : IHolidayPreferenceRule
{
    public List<string> Check(HolidayPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayPreferenceMinimumRangeRule(double Min, double Max) : IHolidayPreferenceRule
{
    public List<string> Check(HolidayPreference item)
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

public sealed record HolidayPreferenceBadgeLengthRule(int MaxLength) : IHolidayPreferenceRule
{
    public List<string> Check(HolidayPreference item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday preference.</summary>
public sealed record HolidayPreferenceValidator(List<IHolidayPreferenceRule> Rules)
{
    public List<string> Validate(HolidayPreference item)
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

    public void RequireValid(HolidayPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayPreference> items)
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

/// <summary>Functions of holiday preference rules.</summary>
public static class HolidayPreferenceRules
{
    public static HolidayPreferenceValidator DefaultHolidayPreferenceValidator()
    {
        return new HolidayPreferenceValidator(
            new List<IHolidayPreferenceRule>
            {
                new HolidayPreferenceRequiredTitleRule("holiday preference needs this field"),
                new HolidayPreferenceRequiredBadgeRule("holiday preference needs an entry"),
                new HolidayPreferenceTitleLengthRule(255),
                new HolidayPreferenceMinimumRangeRule(4.0, 1696.8),
                new HolidayPreferenceBadgeLengthRule(120),
            });
    }
}
