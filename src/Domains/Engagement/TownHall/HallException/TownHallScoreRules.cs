namespace Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>A rule a town hall score must pass. Check returns its problems, or none.</summary>
public interface ITownHallScoreRule
{
    List<string> Check(TownHallScore item);
}

public sealed record TownHallScoreRequiredCodeRule(string Message) : ITownHallScoreRule
{
    public List<string> Check(TownHallScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallScoreRequiredContactEmailRule(string Message) : ITownHallScoreRule
{
    public List<string> Check(TownHallScore item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallScoreContactEmailLengthRule(int MaxLength) : ITownHallScoreRule
{
    public List<string> Check(TownHallScore item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall score.</summary>
public sealed record TownHallScoreValidator(List<ITownHallScoreRule> Rules)
{
    public List<string> Validate(TownHallScore item)
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

    public void RequireValid(TownHallScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallScore> items)
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

/// <summary>Functions of town hall score rules.</summary>
public static class TownHallScoreRules
{
    public static TownHallScoreValidator DefaultTownHallScoreValidator()
    {
        return new TownHallScoreValidator(
            new List<ITownHallScoreRule>
            {
                new TownHallScoreRequiredCodeRule("town hall score needs this field"),
                new TownHallScoreRequiredContactEmailRule("town hall score needs an entry"),
                new TownHallScoreContactEmailLengthRule(80),
            });
    }
}
