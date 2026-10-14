namespace Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>A rule a town hall request must pass. Check returns its problems, or none.</summary>
public interface ITownHallRequestRule
{
    List<string> Check(TownHallRequest item);
}

public sealed record TownHallRequestCapacityRangeRule(int Min, int Max) : ITownHallRequestRule
{
    public List<string> Check(TownHallRequest item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallRequestRequiredNotesRule(string Message) : ITownHallRequestRule
{
    public List<string> Check(TownHallRequest item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallRequestRequiredLabelRule(string Message) : ITownHallRequestRule
{
    public List<string> Check(TownHallRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallRequestRequiredPhoneRule(string Message) : ITownHallRequestRule
{
    public List<string> Check(TownHallRequest item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall request.</summary>
public sealed record TownHallRequestValidator(List<ITownHallRequestRule> Rules)
{
    public List<string> Validate(TownHallRequest item)
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

    public void RequireValid(TownHallRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallRequest> items)
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

/// <summary>Functions of town hall request rules.</summary>
public static class TownHallRequestRules
{
    public static TownHallRequestValidator DefaultTownHallRequestValidator()
    {
        return new TownHallRequestValidator(
            new List<ITownHallRequestRule>
            {
                new TownHallRequestCapacityRangeRule(120, 3600),
                new TownHallRequestRequiredNotesRule("town hall request needs a value"),
                new TownHallRequestRequiredLabelRule("town hall request needs an entry"),
                new TownHallRequestRequiredPhoneRule("town hall request needs a value"),
            });
    }
}
