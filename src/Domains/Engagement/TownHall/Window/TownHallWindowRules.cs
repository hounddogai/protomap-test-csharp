namespace Acme.Workforce.Engagement.TownHall.Window;

/// <summary>A rule a town hall window must pass. Check returns its problems, or none.</summary>
public interface ITownHallWindowRule
{
    List<string> Check(TownHallWindow item);
}

public sealed record TownHallWindowEndsAtRangeRule(int Min, int Max) : ITownHallWindowRule
{
    public List<string> Check(TownHallWindow item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TownHallWindowCodeLengthRule(int MaxLength) : ITownHallWindowRule
{
    public List<string> Check(TownHallWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallWindowRequiredCodeRule(string Message) : ITownHallWindowRule
{
    public List<string> Check(TownHallWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallWindowActiveGuardRule(int Limit) : ITownHallWindowRule
{
    public List<string> Check(TownHallWindow item)
    {
        if (item.Active && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record TownHallWindowSourceLengthRule(int MaxLength) : ITownHallWindowRule
{
    public List<string> Check(TownHallWindow item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall window.</summary>
public sealed record TownHallWindowValidator(List<ITownHallWindowRule> Rules)
{
    public List<string> Validate(TownHallWindow item)
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

    public void RequireValid(TownHallWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallWindow> items)
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

/// <summary>Functions of town hall window rules.</summary>
public static class TownHallWindowRules
{
    public static TownHallWindowValidator DefaultTownHallWindowValidator()
    {
        return new TownHallWindowValidator(
            new List<ITownHallWindowRule>
            {
                new TownHallWindowEndsAtRangeRule(16, 80),
                new TownHallWindowCodeLengthRule(120),
                new TownHallWindowRequiredCodeRule("town hall window needs an entry"),
                new TownHallWindowActiveGuardRule(1000),
                new TownHallWindowSourceLengthRule(3600),
            });
    }
}
