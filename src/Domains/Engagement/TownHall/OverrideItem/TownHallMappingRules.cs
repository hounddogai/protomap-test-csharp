namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

/// <summary>A rule a town hall mapping must pass. Check returns its problems, or none.</summary>
public interface ITownHallMappingRule
{
    List<string> Check(TownHallMapping item);
}

public sealed record TownHallMappingRequiredNotesRule(string Message) : ITownHallMappingRule
{
    public List<string> Check(TownHallMapping item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallMappingRequiredCodeRule(string Message) : ITownHallMappingRule
{
    public List<string> Check(TownHallMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallMappingNotesLengthRule(int MaxLength) : ITownHallMappingRule
{
    public List<string> Check(TownHallMapping item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallMappingRequiredTitleRule(string Message) : ITownHallMappingRule
{
    public List<string> Check(TownHallMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallMappingAttemptsRangeRule(int Min, int Max) : ITownHallMappingRule
{
    public List<string> Check(TownHallMapping item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a town hall mapping.</summary>
public sealed record TownHallMappingValidator(List<ITownHallMappingRule> Rules)
{
    public List<string> Validate(TownHallMapping item)
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

    public void RequireValid(TownHallMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallMapping> items)
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

/// <summary>Functions of town hall mapping rules.</summary>
public static class TownHallMappingRules
{
    public static TownHallMappingValidator DefaultTownHallMappingValidator()
    {
        return new TownHallMappingValidator(
            new List<ITownHallMappingRule>
            {
                new TownHallMappingRequiredNotesRule("town hall mapping needs a value"),
                new TownHallMappingRequiredCodeRule("town hall mapping needs an entry"),
                new TownHallMappingNotesLengthRule(3600),
                new TownHallMappingRequiredTitleRule("town hall mapping needs this field"),
                new TownHallMappingAttemptsRangeRule(120, 120),
            });
    }
}
