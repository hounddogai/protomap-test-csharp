namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict reorg tier must pass. Check returns its problems, or none.</summary>
public interface IStrictReorgTierRule
{
    List<string> Check(StrictReorgTier item);
}

public sealed record StrictReorgTierRequiredTitleRule(string Message) : IStrictReorgTierRule
{
    public List<string> Check(StrictReorgTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgTierAllowedLocationRule(List<string> Allowed) : IStrictReorgTierRule
{
    public List<string> Check(StrictReorgTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgTierRequiredLocationRule(string Message) : IStrictReorgTierRule
{
    public List<string> Check(StrictReorgTier item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictReorgTierAmountRangeRule(double Min, double Max) : IStrictReorgTierRule
{
    public List<string> Check(StrictReorgTier item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict reorg tier.</summary>
public sealed record StrictReorgTierValidator(List<IStrictReorgTierRule> Rules)
{
    public List<string> Validate(StrictReorgTier item)
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

    public void RequireValid(StrictReorgTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictReorgTier> items)
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

/// <summary>Functions of strict reorg tier rules.</summary>
public static class StrictReorgTierRules
{
    public static StrictReorgTierValidator DefaultStrictReorgTierValidator()
    {
        return new StrictReorgTierValidator(
            new List<IStrictReorgTierRule>
            {
                new StrictReorgTierRequiredTitleRule("strict reorg tier needs a value"),
                new StrictReorgTierAllowedLocationRule(new List<string> { "lenient" }),
                new StrictReorgTierRequiredLocationRule("strict reorg tier needs an entry"),
                new StrictReorgTierAmountRangeRule(9.9, 3504.1),
            });
    }
}
