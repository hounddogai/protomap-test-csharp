namespace Acme.Workforce.Absence.Holiday.Window;

/// <summary>A rule a lenient holiday window must pass. Check returns its problems, or none.</summary>
public interface ILenientHolidayWindowRule
{
    List<string> Check(LenientHolidayWindow item);
}

public sealed record LenientHolidayWindowRevisionRangeRule(int Min, int Max) : ILenientHolidayWindowRule
{
    public List<string> Check(LenientHolidayWindow item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientHolidayWindowSourceLengthRule(int MaxLength) : ILenientHolidayWindowRule
{
    public List<string> Check(LenientHolidayWindow item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientHolidayWindowNameLengthRule(int MaxLength) : ILenientHolidayWindowRule
{
    public List<string> Check(LenientHolidayWindow item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient holiday window.</summary>
public sealed record LenientHolidayWindowValidator(List<ILenientHolidayWindowRule> Rules)
{
    public List<string> Validate(LenientHolidayWindow item)
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

    public void RequireValid(LenientHolidayWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientHolidayWindow> items)
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

/// <summary>Functions of lenient holiday window rules.</summary>
public static class LenientHolidayWindowRules
{
    public static LenientHolidayWindowValidator DefaultLenientHolidayWindowValidator()
    {
        return new LenientHolidayWindowValidator(
            new List<ILenientHolidayWindowRule>
            {
                new LenientHolidayWindowRevisionRangeRule(64, 120),
                new LenientHolidayWindowSourceLengthRule(16),
                new LenientHolidayWindowNameLengthRule(1000),
            });
    }
}
