namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>A rule a scenario tier must pass. Check returns its problems, or none.</summary>
public interface IScenarioTierRule
{
    List<string> Check(ScenarioTier item);
}

public sealed record ScenarioTierCodeLengthRule(int MaxLength) : IScenarioTierRule
{
    public List<string> Check(ScenarioTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTierCodePrefixRule(string Prefix) : IScenarioTierRule
{
    public List<string> Check(ScenarioTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTierEndsAtRangeRule(int Min, int Max) : IScenarioTierRule
{
    public List<string> Check(ScenarioTier item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTierWindowDaysRangeRule(int Min, int Max) : IScenarioTierRule
{
    public List<string> Check(ScenarioTier item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioTierRequiredTimezoneRule(string Message) : IScenarioTierRule
{
    public List<string> Check(ScenarioTier item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioTierRequiredUnitRule(string Message) : IScenarioTierRule
{
    public List<string> Check(ScenarioTier item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario tier.</summary>
public sealed record ScenarioTierValidator(List<IScenarioTierRule> Rules)
{
    public List<string> Validate(ScenarioTier item)
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

    public void RequireValid(ScenarioTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioTier> items)
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

/// <summary>Functions of scenario tier rules.</summary>
public static class ScenarioTierRules
{
    public static ScenarioTierValidator DefaultScenarioTierValidator()
    {
        return new ScenarioTierValidator(
            new List<IScenarioTierRule>
            {
                new ScenarioTierCodeLengthRule(80),
                new ScenarioTierCodePrefixRule("Q"),
                new ScenarioTierEndsAtRangeRule(80, 1000),
                new ScenarioTierWindowDaysRangeRule(32, 1000),
                new ScenarioTierRequiredTimezoneRule("scenario tier needs an entry"),
                new ScenarioTierRequiredUnitRule("scenario tier needs a value"),
            });
    }
}
