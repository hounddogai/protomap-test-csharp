namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario mapping must pass. Check returns its problems, or none.</summary>
public interface IScenarioMappingRule
{
    List<string> Check(ScenarioMapping item);
}

public sealed record ScenarioMappingCodeLengthRule(int MaxLength) : IScenarioMappingRule
{
    public List<string> Check(ScenarioMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioMappingRequiredCodeRule(string Message) : IScenarioMappingRule
{
    public List<string> Check(ScenarioMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioMappingRequiredNameRule(string Message) : IScenarioMappingRule
{
    public List<string> Check(ScenarioMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioMappingCodePrefixRule(string Prefix) : IScenarioMappingRule
{
    public List<string> Check(ScenarioMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioMappingNameLengthRule(int MaxLength) : IScenarioMappingRule
{
    public List<string> Check(ScenarioMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioMappingAllowedNameRule(List<string> Allowed) : IScenarioMappingRule
{
    public List<string> Check(ScenarioMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario mapping.</summary>
public sealed record ScenarioMappingValidator(List<IScenarioMappingRule> Rules)
{
    public List<string> Validate(ScenarioMapping item)
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

    public void RequireValid(ScenarioMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioMapping> items)
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

/// <summary>Functions of scenario mapping rules.</summary>
public static class ScenarioMappingRules
{
    public static ScenarioMappingValidator DefaultScenarioMappingValidator()
    {
        return new ScenarioMappingValidator(
            new List<IScenarioMappingRule>
            {
                new ScenarioMappingCodeLengthRule(32),
                new ScenarioMappingRequiredCodeRule("scenario mapping needs this field"),
                new ScenarioMappingRequiredNameRule("scenario mapping needs a value"),
                new ScenarioMappingCodePrefixRule("Q"),
                new ScenarioMappingNameLengthRule(32),
                new ScenarioMappingAllowedNameRule(new List<string> { "primary" }),
            });
    }
}
