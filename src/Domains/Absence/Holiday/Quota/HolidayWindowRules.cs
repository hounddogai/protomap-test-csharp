namespace Acme.Workforce.Absence.Holiday.Quota;

/// <summary>A rule a holiday window must pass. Check returns its problems, or none.</summary>
public interface IHolidayWindowRule
{
    List<string> Check(HolidayWindow item);
}

public sealed record HolidayWindowCodePrefixRule(string Prefix) : IHolidayWindowRule
{
    public List<string> Check(HolidayWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayWindowDurationMinutesRangeRule(int Min, int Max) : IHolidayWindowRule
{
    public List<string> Check(HolidayWindow item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HolidayWindowMinimumRangeRule(double Min, double Max) : IHolidayWindowRule
{
    public List<string> Check(HolidayWindow item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayWindowRequiredNameRule(string Message) : IHolidayWindowRule
{
    public List<string> Check(HolidayWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayWindowNameLengthRule(int MaxLength) : IHolidayWindowRule
{
    public List<string> Check(HolidayWindow item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayWindowRequiredCodeRule(string Message) : IHolidayWindowRule
{
    public List<string> Check(HolidayWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday window.</summary>
public sealed record HolidayWindowValidator(List<IHolidayWindowRule> Rules)
{
    public List<string> Validate(HolidayWindow item)
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

    public void RequireValid(HolidayWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayWindow> items)
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

/// <summary>Functions of holiday window rules.</summary>
public static class HolidayWindowRules
{
    public static HolidayWindowValidator DefaultHolidayWindowValidator()
    {
        return new HolidayWindowValidator(
            new List<IHolidayWindowRule>
            {
                new HolidayWindowCodePrefixRule("ACM"),
                new HolidayWindowDurationMinutesRangeRule(32, 64),
                new HolidayWindowMinimumRangeRule(3.8, 3310.9),
                new HolidayWindowRequiredNameRule("holiday window needs this field"),
                new HolidayWindowNameLengthRule(32),
                new HolidayWindowRequiredCodeRule("holiday window needs an entry"),
            });
    }
}
