namespace Acme.Workforce.Orgdesign.Scenario.Segment;

/// <summary>A rule a scenario policy must pass. Check returns its problems, or none.</summary>
public interface IScenarioPolicyRule
{
    List<string> Check(ScenarioPolicy item);
}

public sealed record ScenarioPolicyTeamLengthRule(int MaxLength) : IScenarioPolicyRule
{
    public List<string> Check(ScenarioPolicy item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioPolicyCodePrefixRule(string Prefix) : IScenarioPolicyRule
{
    public List<string> Check(ScenarioPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioPolicyVisibleGuardRule(double Limit) : IScenarioPolicyRule
{
    public List<string> Check(ScenarioPolicy item)
    {
        if (item.Visible && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioPolicyRequiredNameRule(string Message) : IScenarioPolicyRule
{
    public List<string> Check(ScenarioPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario policy.</summary>
public sealed record ScenarioPolicyValidator(List<IScenarioPolicyRule> Rules)
{
    public List<string> Validate(ScenarioPolicy item)
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

    public void RequireValid(ScenarioPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioPolicy> items)
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

/// <summary>Functions of scenario policy rules.</summary>
public static class ScenarioPolicyRules
{
    public static ScenarioPolicyValidator DefaultScenarioPolicyValidator()
    {
        return new ScenarioPolicyValidator(
            new List<IScenarioPolicyRule>
            {
                new ScenarioPolicyTeamLengthRule(80),
                new ScenarioPolicyCodePrefixRule("OPS"),
                new ScenarioPolicyVisibleGuardRule(4.6),
                new ScenarioPolicyRequiredNameRule("scenario policy needs a value"),
            });
    }
}
