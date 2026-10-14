namespace Acme.Workforce.Workplace.Desk.DeskProfile;

/// <summary>A rule a lenient desk profile must pass. Check returns its problems, or none.</summary>
public interface ILenientDeskProfileRule
{
    List<string> Check(LenientDeskProfile item);
}

public sealed record LenientDeskProfileFloorValueRangeRule(double Min, double Max) : ILenientDeskProfileRule
{
    public List<string> Check(LenientDeskProfile item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskProfileNotesLengthRule(int MaxLength) : ILenientDeskProfileRule
{
    public List<string> Check(LenientDeskProfile item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskProfileLeaveDaysRangeRule(int Min, int Max) : ILenientDeskProfileRule
{
    public List<string> Check(LenientDeskProfile item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientDeskProfileCodeLengthRule(int MaxLength) : ILenientDeskProfileRule
{
    public List<string> Check(LenientDeskProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskProfileRequiredNotesRule(string Message) : ILenientDeskProfileRule
{
    public List<string> Check(LenientDeskProfile item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientDeskProfilePhoneLengthRule(int MaxLength) : ILenientDeskProfileRule
{
    public List<string> Check(LenientDeskProfile item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient desk profile.</summary>
public sealed record LenientDeskProfileValidator(List<ILenientDeskProfileRule> Rules)
{
    public List<string> Validate(LenientDeskProfile item)
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

    public void RequireValid(LenientDeskProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientDeskProfile> items)
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

/// <summary>Functions of lenient desk profile rules.</summary>
public static class LenientDeskProfileRules
{
    public static LenientDeskProfileValidator DefaultLenientDeskProfileValidator()
    {
        return new LenientDeskProfileValidator(
            new List<ILenientDeskProfileRule>
            {
                new LenientDeskProfileFloorValueRangeRule(0.3, 1121.8),
                new LenientDeskProfileNotesLengthRule(255),
                new LenientDeskProfileLeaveDaysRangeRule(255, 255),
                new LenientDeskProfileCodeLengthRule(80),
                new LenientDeskProfileRequiredNotesRule("lenient desk profile needs a value"),
                new LenientDeskProfilePhoneLengthRule(120),
            });
    }
}
