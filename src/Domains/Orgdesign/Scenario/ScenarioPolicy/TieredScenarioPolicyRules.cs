namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered scenario policy must pass. Check returns its problems, or none.</summary>
public interface ITieredScenarioPolicyRule
{
    List<string> Check(TieredScenarioPolicy item);
}

public sealed record TieredScenarioPolicyAllowedCodeRule(List<string> Allowed) : ITieredScenarioPolicyRule
{
    public List<string> Check(TieredScenarioPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TieredScenarioPolicyCodePrefixRule(string Prefix) : ITieredScenarioPolicyRule
{
    public List<string> Check(TieredScenarioPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredScenarioPolicyRequiredTitleRule(string Message) : ITieredScenarioPolicyRule
{
    public List<string> Check(TieredScenarioPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredScenarioPolicyStartsAtRangeRule(int Min, int Max) : ITieredScenarioPolicyRule
{
    public List<string> Check(TieredScenarioPolicy item)
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

public sealed record TieredScenarioPolicyRateRangeRule(double Min, double Max) : ITieredScenarioPolicyRule
{
    public List<string> Check(TieredScenarioPolicy item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered scenario policy.</summary>
public sealed record TieredScenarioPolicyValidator(List<ITieredScenarioPolicyRule> Rules)
{
    public List<string> Validate(TieredScenarioPolicy item)
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

    public void RequireValid(TieredScenarioPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredScenarioPolicy> items)
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

/// <summary>Functions of tiered scenario policy rules.</summary>
public static class TieredScenarioPolicyRules
{
    public static TieredScenarioPolicyValidator DefaultTieredScenarioPolicyValidator()
    {
        return new TieredScenarioPolicyValidator(
            new List<ITieredScenarioPolicyRule>
            {
                new TieredScenarioPolicyAllowedCodeRule(new List<string>()),
                new TieredScenarioPolicyCodePrefixRule("REF"),
                new TieredScenarioPolicyRequiredTitleRule("tiered scenario policy needs this field"),
                new TieredScenarioPolicyStartsAtRangeRule(32, 3600),
                new TieredScenarioPolicyRateRangeRule(2.0, 500.1),
            });
    }
}
