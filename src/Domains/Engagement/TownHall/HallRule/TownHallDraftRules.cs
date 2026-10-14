namespace Acme.Workforce.Engagement.TownHall.HallRule;

/// <summary>A rule a town hall draft must pass. Check returns its problems, or none.</summary>
public interface ITownHallDraftRule
{
    List<string> Check(TownHallDraft item);
}

public sealed record TownHallDraftCostRangeRule(double Min, double Max) : ITownHallDraftRule
{
    public List<string> Check(TownHallDraft item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TownHallDraftCodePrefixRule(string Prefix) : ITownHallDraftRule
{
    public List<string> Check(TownHallDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallDraftRequiredCodeRule(string Message) : ITownHallDraftRule
{
    public List<string> Check(TownHallDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallDraftRevisionRangeRule(int Min, int Max) : ITownHallDraftRule
{
    public List<string> Check(TownHallDraft item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallDraftTagsLimitRule(int MaxCount, List<string> Banned) : ITownHallDraftRule
{
    public List<string> Check(TownHallDraft item)
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

/// <summary>Runs every rule of a town hall draft.</summary>
public sealed record TownHallDraftValidator(List<ITownHallDraftRule> Rules)
{
    public List<string> Validate(TownHallDraft item)
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

    public void RequireValid(TownHallDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallDraft> items)
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

/// <summary>Functions of town hall draft rules.</summary>
public static class TownHallDraftRules
{
    public static TownHallDraftValidator DefaultTownHallDraftValidator()
    {
        return new TownHallDraftValidator(
            new List<ITownHallDraftRule>
            {
                new TownHallDraftCostRangeRule(1.0, 3665.9),
                new TownHallDraftCodePrefixRule("REF"),
                new TownHallDraftRequiredCodeRule("town hall draft needs a value"),
                new TownHallDraftRevisionRangeRule(16, 32),
                new TownHallDraftTagsLimitRule(3600, new List<string> { "legacy", "adaptive", "strict" }),
            });
    }
}
