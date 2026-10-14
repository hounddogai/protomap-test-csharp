namespace Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>A rule a town hall baseline must pass. Check returns its problems, or none.</summary>
public interface ITownHallBaselineRule
{
    List<string> Check(TownHallBaseline item);
}

public sealed record TownHallBaselineRequiredSourceRule(string Message) : ITownHallBaselineRule
{
    public List<string> Check(TownHallBaseline item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallBaselineRequiredCodeRule(string Message) : ITownHallBaselineRule
{
    public List<string> Check(TownHallBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallBaselineCodePrefixRule(string Prefix) : ITownHallBaselineRule
{
    public List<string> Check(TownHallBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall baseline.</summary>
public sealed record TownHallBaselineValidator(List<ITownHallBaselineRule> Rules)
{
    public List<string> Validate(TownHallBaseline item)
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

    public void RequireValid(TownHallBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallBaseline> items)
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

/// <summary>Functions of town hall baseline rules.</summary>
public static class TownHallBaselineRules
{
    public static TownHallBaselineValidator DefaultTownHallBaselineValidator()
    {
        return new TownHallBaselineValidator(
            new List<ITownHallBaselineRule>
            {
                new TownHallBaselineRequiredSourceRule("town hall baseline needs a value"),
                new TownHallBaselineRequiredCodeRule("town hall baseline needs this field"),
                new TownHallBaselineCodePrefixRule("REF"),
            });
    }
}
