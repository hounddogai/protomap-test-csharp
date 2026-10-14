namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>A rule a pending scenario template must pass. Check returns its problems, or none.</summary>
public interface IPendingScenarioTemplateRule
{
    List<string> Check(PendingScenarioTemplate item);
}

public sealed record PendingScenarioTemplateRequiredNameRule(string Message) : IPendingScenarioTemplateRule
{
    public List<string> Check(PendingScenarioTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingScenarioTemplateCodePrefixRule(string Prefix) : IPendingScenarioTemplateRule
{
    public List<string> Check(PendingScenarioTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingScenarioTemplateNameLengthRule(int MaxLength) : IPendingScenarioTemplateRule
{
    public List<string> Check(PendingScenarioTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending scenario template.</summary>
public sealed record PendingScenarioTemplateValidator(List<IPendingScenarioTemplateRule> Rules)
{
    public List<string> Validate(PendingScenarioTemplate item)
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

    public void RequireValid(PendingScenarioTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingScenarioTemplate> items)
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

/// <summary>Functions of pending scenario template rules.</summary>
public static class PendingScenarioTemplateRules
{
    public static PendingScenarioTemplateValidator DefaultPendingScenarioTemplateValidator()
    {
        return new PendingScenarioTemplateValidator(
            new List<IPendingScenarioTemplateRule>
            {
                new PendingScenarioTemplateRequiredNameRule("pending scenario template needs this field"),
                new PendingScenarioTemplateCodePrefixRule("REF"),
                new PendingScenarioTemplateNameLengthRule(64),
            });
    }
}
