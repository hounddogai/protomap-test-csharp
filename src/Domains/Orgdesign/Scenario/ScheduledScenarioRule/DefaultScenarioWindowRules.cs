namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>A rule a default scenario window must pass. Check returns its problems, or none.</summary>
public interface IDefaultScenarioWindowRule
{
    List<string> Check(DefaultScenarioWindow item);
}

public sealed record DefaultScenarioWindowCodePrefixRule(string Prefix) : IDefaultScenarioWindowRule
{
    public List<string> Check(DefaultScenarioWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultScenarioWindowRequiredLabelRule(string Message) : IDefaultScenarioWindowRule
{
    public List<string> Check(DefaultScenarioWindow item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultScenarioWindowPinnedGuardRule(double Limit) : IDefaultScenarioWindowRule
{
    public List<string> Check(DefaultScenarioWindow item)
    {
        if (item.Pinned && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default scenario window.</summary>
public sealed record DefaultScenarioWindowValidator(List<IDefaultScenarioWindowRule> Rules)
{
    public List<string> Validate(DefaultScenarioWindow item)
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

    public void RequireValid(DefaultScenarioWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultScenarioWindow> items)
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

/// <summary>Functions of default scenario window rules.</summary>
public static class DefaultScenarioWindowRules
{
    public static DefaultScenarioWindowValidator DefaultDefaultScenarioWindowValidator()
    {
        return new DefaultScenarioWindowValidator(
            new List<IDefaultScenarioWindowRule>
            {
                new DefaultScenarioWindowCodePrefixRule("OPS"),
                new DefaultScenarioWindowRequiredLabelRule("default scenario window needs an entry"),
                new DefaultScenarioWindowPinnedGuardRule(6.4),
            });
    }
}
