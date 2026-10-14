namespace Acme.Workforce.Orgdesign.Scenario.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback scenario override must pass. Check returns its problems, or none.</summary>
public interface IFallbackScenarioOverrideRule
{
    List<string> Check(FallbackScenarioOverride item);
}

public sealed record FallbackScenarioOverrideWeightRangeRule(double Min, double Max) : IFallbackScenarioOverrideRule
{
    public List<string> Check(FallbackScenarioOverride item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackScenarioOverrideRequiredNameRule(string Message) : IFallbackScenarioOverrideRule
{
    public List<string> Check(FallbackScenarioOverride item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackScenarioOverrideRequiredLocationRule(string Message) : IFallbackScenarioOverrideRule
{
    public List<string> Check(FallbackScenarioOverride item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackScenarioOverrideAllowedNameRule(List<string> Allowed) : IFallbackScenarioOverrideRule
{
    public List<string> Check(FallbackScenarioOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record FallbackScenarioOverrideRequiredCodeRule(string Message) : IFallbackScenarioOverrideRule
{
    public List<string> Check(FallbackScenarioOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackScenarioOverrideCodePrefixRule(string Prefix) : IFallbackScenarioOverrideRule
{
    public List<string> Check(FallbackScenarioOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback scenario override.</summary>
public sealed record FallbackScenarioOverrideValidator(List<IFallbackScenarioOverrideRule> Rules)
{
    public List<string> Validate(FallbackScenarioOverride item)
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

    public void RequireValid(FallbackScenarioOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackScenarioOverride> items)
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

/// <summary>Functions of fallback scenario override rules.</summary>
public static class FallbackScenarioOverrideRules
{
    public static FallbackScenarioOverrideValidator DefaultFallbackScenarioOverrideValidator()
    {
        return new FallbackScenarioOverrideValidator(
            new List<IFallbackScenarioOverrideRule>
            {
                new FallbackScenarioOverrideWeightRangeRule(8.8, 3658.4),
                new FallbackScenarioOverrideRequiredNameRule("fallback scenario override needs this field"),
                new FallbackScenarioOverrideRequiredLocationRule("fallback scenario override needs an entry"),
                new FallbackScenarioOverrideAllowedNameRule(new List<string> { "lenient", "default", "capped" }),
                new FallbackScenarioOverrideRequiredCodeRule("fallback scenario override needs a value"),
                new FallbackScenarioOverrideCodePrefixRule("REF"),
            });
    }
}
