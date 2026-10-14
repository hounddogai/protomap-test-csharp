namespace Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>A rule a seasonal succession plan rule must pass. Check returns its problems, or none.</summary>
public interface ISeasonalSuccessionPlanRuleRule
{
    List<string> Check(SeasonalSuccessionPlanRule item);
}

public sealed record SeasonalSuccessionPlanRuleCodeLengthRule(int MaxLength) : ISeasonalSuccessionPlanRuleRule
{
    public List<string> Check(SeasonalSuccessionPlanRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSuccessionPlanRuleQuantityRangeRule(int Min, int Max) : ISeasonalSuccessionPlanRuleRule
{
    public List<string> Check(SeasonalSuccessionPlanRule item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalSuccessionPlanRuleCodePrefixRule(string Prefix) : ISeasonalSuccessionPlanRuleRule
{
    public List<string> Check(SeasonalSuccessionPlanRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSuccessionPlanRulePinnedGuardRule(int Limit) : ISeasonalSuccessionPlanRuleRule
{
    public List<string> Check(SeasonalSuccessionPlanRule item)
    {
        if (item.Pinned && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSuccessionPlanRuleLimitRangeRule(int Min, int Max) : ISeasonalSuccessionPlanRuleRule
{
    public List<string> Check(SeasonalSuccessionPlanRule item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSuccessionPlanRuleRequiredTitleRule(string Message) : ISeasonalSuccessionPlanRuleRule
{
    public List<string> Check(SeasonalSuccessionPlanRule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal succession plan rule.</summary>
public sealed record SeasonalSuccessionPlanRuleValidator(List<ISeasonalSuccessionPlanRuleRule> Rules)
{
    public List<string> Validate(SeasonalSuccessionPlanRule item)
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

    public void RequireValid(SeasonalSuccessionPlanRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalSuccessionPlanRule> items)
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

/// <summary>Functions of seasonal succession plan rule rules.</summary>
public static class SeasonalSuccessionPlanRuleRules
{
    public static SeasonalSuccessionPlanRuleValidator DefaultSeasonalSuccessionPlanRuleValidator()
    {
        return new SeasonalSuccessionPlanRuleValidator(
            new List<ISeasonalSuccessionPlanRuleRule>
            {
                new SeasonalSuccessionPlanRuleCodeLengthRule(16),
                new SeasonalSuccessionPlanRuleQuantityRangeRule(1000, 3600),
                new SeasonalSuccessionPlanRuleCodePrefixRule("Q"),
                new SeasonalSuccessionPlanRulePinnedGuardRule(32),
                new SeasonalSuccessionPlanRuleLimitRangeRule(64, 1000),
                new SeasonalSuccessionPlanRuleRequiredTitleRule("seasonal succession plan rule needs an entry"),
            });
    }
}
