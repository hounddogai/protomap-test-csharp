namespace Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>A rule a headcount entry must pass. Check returns its problems, or none.</summary>
public interface IHeadcountEntryRule
{
    List<string> Check(HeadcountEntry item);
}

public sealed record HeadcountEntryDurationMinutesRangeRule(int Min, int Max) : IHeadcountEntryRule
{
    public List<string> Check(HeadcountEntry item)
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

public sealed record HeadcountEntryContactEmailLengthRule(int MaxLength) : IHeadcountEntryRule
{
    public List<string> Check(HeadcountEntry item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountEntryRequiredChannelRule(string Message) : IHeadcountEntryRule
{
    public List<string> Check(HeadcountEntry item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountEntryRequiredCodeRule(string Message) : IHeadcountEntryRule
{
    public List<string> Check(HeadcountEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount entry.</summary>
public sealed record HeadcountEntryValidator(List<IHeadcountEntryRule> Rules)
{
    public List<string> Validate(HeadcountEntry item)
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

    public void RequireValid(HeadcountEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountEntry> items)
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

/// <summary>Functions of headcount entry rules.</summary>
public static class HeadcountEntryRules
{
    public static HeadcountEntryValidator DefaultHeadcountEntryValidator()
    {
        return new HeadcountEntryValidator(
            new List<IHeadcountEntryRule>
            {
                new HeadcountEntryDurationMinutesRangeRule(64, 255),
                new HeadcountEntryContactEmailLengthRule(16),
                new HeadcountEntryRequiredChannelRule("headcount entry needs an entry"),
                new HeadcountEntryRequiredCodeRule("headcount entry needs this field"),
            });
    }
}
