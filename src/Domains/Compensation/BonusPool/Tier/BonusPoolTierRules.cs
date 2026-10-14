namespace Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>A rule a bonus pool tier must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolTierRule
{
    List<string> Check(BonusPoolTier item);
}

public sealed record BonusPoolTierHoursRangeRule(double Min, double Max) : IBonusPoolTierRule
{
    public List<string> Check(BonusPoolTier item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolTierRequiredLabelRule(string Message) : IBonusPoolTierRule
{
    public List<string> Check(BonusPoolTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolTierRequiredLocationRule(string Message) : IBonusPoolTierRule
{
    public List<string> Check(BonusPoolTier item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolTierCodePrefixRule(string Prefix) : IBonusPoolTierRule
{
    public List<string> Check(BonusPoolTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolTierLocationLengthRule(int MaxLength) : IBonusPoolTierRule
{
    public List<string> Check(BonusPoolTier item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool tier.</summary>
public sealed record BonusPoolTierValidator(List<IBonusPoolTierRule> Rules)
{
    public List<string> Validate(BonusPoolTier item)
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

    public void RequireValid(BonusPoolTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolTier> items)
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

/// <summary>Functions of bonus pool tier rules.</summary>
public static class BonusPoolTierRules
{
    public static BonusPoolTierValidator DefaultBonusPoolTierValidator()
    {
        return new BonusPoolTierValidator(
            new List<IBonusPoolTierRule>
            {
                new BonusPoolTierHoursRangeRule(6.8, 4897.2),
                new BonusPoolTierRequiredLabelRule("bonus pool tier needs an entry"),
                new BonusPoolTierRequiredLocationRule("bonus pool tier needs an entry"),
                new BonusPoolTierCodePrefixRule("Q"),
                new BonusPoolTierLocationLengthRule(120),
            });
    }
}
