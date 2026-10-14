namespace Acme.Workforce.Engagement.TownHall.OverrideItem;

/// <summary>A rule a town hall override must pass. Check returns its problems, or none.</summary>
public interface ITownHallOverrideRule
{
    List<string> Check(TownHallOverride item);
}

public sealed record TownHallOverrideTitleLengthRule(int MaxLength) : ITownHallOverrideRule
{
    public List<string> Check(TownHallOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallOverrideCodePrefixRule(string Prefix) : ITownHallOverrideRule
{
    public List<string> Check(TownHallOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallOverrideRequiredTitleRule(string Message) : ITownHallOverrideRule
{
    public List<string> Check(TownHallOverride item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallOverrideRequiredCodeRule(string Message) : ITownHallOverrideRule
{
    public List<string> Check(TownHallOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall override.</summary>
public sealed record TownHallOverrideValidator(List<ITownHallOverrideRule> Rules)
{
    public List<string> Validate(TownHallOverride item)
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

    public void RequireValid(TownHallOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallOverride> items)
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

/// <summary>Functions of town hall override rules.</summary>
public static class TownHallOverrideRules
{
    public static TownHallOverrideValidator DefaultTownHallOverrideValidator()
    {
        return new TownHallOverrideValidator(
            new List<ITownHallOverrideRule>
            {
                new TownHallOverrideTitleLengthRule(80),
                new TownHallOverrideCodePrefixRule("REF"),
                new TownHallOverrideRequiredTitleRule("town hall override needs a value"),
                new TownHallOverrideRequiredCodeRule("town hall override needs this field"),
            });
    }
}
