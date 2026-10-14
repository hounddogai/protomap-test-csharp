namespace Acme.Workforce.Engagement.TownHall.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall tier must pass. Check returns its problems, or none.</summary>
public interface ITownHallTierRule
{
    List<string> Check(TownHallTier item);
}

public sealed record TownHallTierAllowedCodeRule(List<string> Allowed) : ITownHallTierRule
{
    public List<string> Check(TownHallTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record TownHallTierMarginRangeRule(double Min, double Max) : ITownHallTierRule
{
    public List<string> Check(TownHallTier item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallTierRequiredLabelRule(string Message) : ITownHallTierRule
{
    public List<string> Check(TownHallTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallTierLabelLengthRule(int MaxLength) : ITownHallTierRule
{
    public List<string> Check(TownHallTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallTierRequiredTeamRule(string Message) : ITownHallTierRule
{
    public List<string> Check(TownHallTier item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall tier.</summary>
public sealed record TownHallTierValidator(List<ITownHallTierRule> Rules)
{
    public List<string> Validate(TownHallTier item)
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

    public void RequireValid(TownHallTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallTier> items)
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

/// <summary>Functions of town hall tier rules.</summary>
public static class TownHallTierRules
{
    public static TownHallTierValidator DefaultTownHallTierValidator()
    {
        return new TownHallTierValidator(
            new List<ITownHallTierRule>
            {
                new TownHallTierAllowedCodeRule(new List<string> { "adaptive", "primary" }),
                new TownHallTierMarginRangeRule(9.6, 4055.8),
                new TownHallTierRequiredLabelRule("town hall tier needs a value"),
                new TownHallTierLabelLengthRule(64),
                new TownHallTierRequiredTeamRule("town hall tier needs an entry"),
            });
    }
}
