namespace Acme.Workforce.Orgdesign.Scenario.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal scenario tier must pass. Check returns its problems, or none.</summary>
public interface ISeasonalScenarioTierRule
{
    List<string> Check(SeasonalScenarioTier item);
}

public sealed record SeasonalScenarioTierCodeLengthRule(int MaxLength) : ISeasonalScenarioTierRule
{
    public List<string> Check(SeasonalScenarioTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalScenarioTierRequiredDescriptionRule(string Message) : ISeasonalScenarioTierRule
{
    public List<string> Check(SeasonalScenarioTier item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalScenarioTierDescriptionLengthRule(int MaxLength) : ISeasonalScenarioTierRule
{
    public List<string> Check(SeasonalScenarioTier item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalScenarioTierRequiredNameRule(string Message) : ISeasonalScenarioTierRule
{
    public List<string> Check(SeasonalScenarioTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalScenarioTierRequiredCodeRule(string Message) : ISeasonalScenarioTierRule
{
    public List<string> Check(SeasonalScenarioTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalScenarioTierAllowedDescriptionRule(List<string> Allowed) : ISeasonalScenarioTierRule
{
    public List<string> Check(SeasonalScenarioTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal scenario tier.</summary>
public sealed record SeasonalScenarioTierValidator(List<ISeasonalScenarioTierRule> Rules)
{
    public List<string> Validate(SeasonalScenarioTier item)
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

    public void RequireValid(SeasonalScenarioTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalScenarioTier> items)
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

/// <summary>Functions of seasonal scenario tier rules.</summary>
public static class SeasonalScenarioTierRules
{
    public static SeasonalScenarioTierValidator DefaultSeasonalScenarioTierValidator()
    {
        return new SeasonalScenarioTierValidator(
            new List<ISeasonalScenarioTierRule>
            {
                new SeasonalScenarioTierCodeLengthRule(32),
                new SeasonalScenarioTierRequiredDescriptionRule("seasonal scenario tier needs a value"),
                new SeasonalScenarioTierDescriptionLengthRule(16),
                new SeasonalScenarioTierRequiredNameRule("seasonal scenario tier needs this field"),
                new SeasonalScenarioTierRequiredCodeRule("seasonal scenario tier needs a value"),
                new SeasonalScenarioTierAllowedDescriptionRule(new List<string>()),
            });
    }
}
