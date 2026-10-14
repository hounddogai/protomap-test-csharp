namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>A rule a holiday target must pass. Check returns its problems, or none.</summary>
public interface IHolidayTargetRule
{
    List<string> Check(HolidayTarget item);
}

public sealed record HolidayTargetLeaveDaysRangeRule(int Min, int Max) : IHolidayTargetRule
{
    public List<string> Check(HolidayTarget item)
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

public sealed record HolidayTargetCodeLengthRule(int MaxLength) : IHolidayTargetRule
{
    public List<string> Check(HolidayTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayTargetRequiredCodeRule(string Message) : IHolidayTargetRule
{
    public List<string> Check(HolidayTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayTargetNameLengthRule(int MaxLength) : IHolidayTargetRule
{
    public List<string> Check(HolidayTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayTargetTagsLimitRule(int MaxCount, List<string> Banned) : IHolidayTargetRule
{
    public List<string> Check(HolidayTarget item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record HolidayTargetRequiredNameRule(string Message) : IHolidayTargetRule
{
    public List<string> Check(HolidayTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday target.</summary>
public sealed record HolidayTargetValidator(List<IHolidayTargetRule> Rules)
{
    public List<string> Validate(HolidayTarget item)
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

    public void RequireValid(HolidayTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayTarget> items)
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

/// <summary>Functions of holiday target rules.</summary>
public static class HolidayTargetRules
{
    public static HolidayTargetValidator DefaultHolidayTargetValidator()
    {
        return new HolidayTargetValidator(
            new List<IHolidayTargetRule>
            {
                new HolidayTargetLeaveDaysRangeRule(16, 3600),
                new HolidayTargetCodeLengthRule(32),
                new HolidayTargetRequiredCodeRule("holiday target needs this field"),
                new HolidayTargetNameLengthRule(1000),
                new HolidayTargetTagsLimitRule(3600, new List<string> { "capped", "regional", "default" }),
                new HolidayTargetRequiredNameRule("holiday target needs an entry"),
            });
    }
}
