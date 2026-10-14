namespace Acme.Workforce.Orgdesign.Scenario.Quota;

/// <summary>A rule a weighted scenario override must pass. Check returns its problems, or none.</summary>
public interface IWeightedScenarioOverrideRule
{
    List<string> Check(WeightedScenarioOverride item);
}

public sealed record WeightedScenarioOverrideRequiredTitleRule(string Message) : IWeightedScenarioOverrideRule
{
    public List<string> Check(WeightedScenarioOverride item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedScenarioOverrideCodePrefixRule(string Prefix) : IWeightedScenarioOverrideRule
{
    public List<string> Check(WeightedScenarioOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedScenarioOverrideRequiredTeamRule(string Message) : IWeightedScenarioOverrideRule
{
    public List<string> Check(WeightedScenarioOverride item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedScenarioOverrideStartsAtRangeRule(int Min, int Max) : IWeightedScenarioOverrideRule
{
    public List<string> Check(WeightedScenarioOverride item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedScenarioOverrideEndsAtRangeRule(int Min, int Max) : IWeightedScenarioOverrideRule
{
    public List<string> Check(WeightedScenarioOverride item)
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

public sealed record WeightedScenarioOverrideTitleLengthRule(int MaxLength) : IWeightedScenarioOverrideRule
{
    public List<string> Check(WeightedScenarioOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted scenario override.</summary>
public sealed record WeightedScenarioOverrideValidator(List<IWeightedScenarioOverrideRule> Rules)
{
    public List<string> Validate(WeightedScenarioOverride item)
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

    public void RequireValid(WeightedScenarioOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedScenarioOverride> items)
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

/// <summary>Functions of weighted scenario override rules.</summary>
public static class WeightedScenarioOverrideRules
{
    public static WeightedScenarioOverrideValidator DefaultWeightedScenarioOverrideValidator()
    {
        return new WeightedScenarioOverrideValidator(
            new List<IWeightedScenarioOverrideRule>
            {
                new WeightedScenarioOverrideRequiredTitleRule("weighted scenario override needs a value"),
                new WeightedScenarioOverrideCodePrefixRule("Q"),
                new WeightedScenarioOverrideRequiredTeamRule("weighted scenario override needs this field"),
                new WeightedScenarioOverrideStartsAtRangeRule(32, 80),
                new WeightedScenarioOverrideEndsAtRangeRule(32, 255),
                new WeightedScenarioOverrideTitleLengthRule(16),
            });
    }
}
