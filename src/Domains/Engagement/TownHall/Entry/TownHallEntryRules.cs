namespace Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>A rule a town hall entry must pass. Check returns its problems, or none.</summary>
public interface ITownHallEntryRule
{
    List<string> Check(TownHallEntry item);
}

public sealed record TownHallEntryNameLengthRule(int MaxLength) : ITownHallEntryRule
{
    public List<string> Check(TownHallEntry item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallEntryRequiredCodeRule(string Message) : ITownHallEntryRule
{
    public List<string> Check(TownHallEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallEntrySequenceRangeRule(int Min, int Max) : ITownHallEntryRule
{
    public List<string> Check(TownHallEntry item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallEntryRequiredPhoneRule(string Message) : ITownHallEntryRule
{
    public List<string> Check(TownHallEntry item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallEntryEmployeeIdLengthRule(int MaxLength) : ITownHallEntryRule
{
    public List<string> Check(TownHallEntry item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall entry.</summary>
public sealed record TownHallEntryValidator(List<ITownHallEntryRule> Rules)
{
    public List<string> Validate(TownHallEntry item)
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

    public void RequireValid(TownHallEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallEntry> items)
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

/// <summary>Functions of town hall entry rules.</summary>
public static class TownHallEntryRules
{
    public static TownHallEntryValidator DefaultTownHallEntryValidator()
    {
        return new TownHallEntryValidator(
            new List<ITownHallEntryRule>
            {
                new TownHallEntryNameLengthRule(120),
                new TownHallEntryRequiredCodeRule("town hall entry needs this field"),
                new TownHallEntrySequenceRangeRule(32, 3600),
                new TownHallEntryRequiredPhoneRule("town hall entry needs an entry"),
                new TownHallEntryEmployeeIdLengthRule(86400),
            });
    }
}
