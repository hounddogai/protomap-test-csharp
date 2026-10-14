namespace Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>A rule a primary scenario mapping must pass. Check returns its problems, or none.</summary>
public interface IPrimaryScenarioMappingRule
{
    List<string> Check(PrimaryScenarioMapping item);
}

public sealed record PrimaryScenarioMappingCodeLengthRule(int MaxLength) : IPrimaryScenarioMappingRule
{
    public List<string> Check(PrimaryScenarioMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioMappingLabelLengthRule(int MaxLength) : IPrimaryScenarioMappingRule
{
    public List<string> Check(PrimaryScenarioMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioMappingCodePrefixRule(string Prefix) : IPrimaryScenarioMappingRule
{
    public List<string> Check(PrimaryScenarioMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioMappingUnitLengthRule(int MaxLength) : IPrimaryScenarioMappingRule
{
    public List<string> Check(PrimaryScenarioMapping item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary scenario mapping.</summary>
public sealed record PrimaryScenarioMappingValidator(List<IPrimaryScenarioMappingRule> Rules)
{
    public List<string> Validate(PrimaryScenarioMapping item)
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

    public void RequireValid(PrimaryScenarioMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryScenarioMapping> items)
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

/// <summary>Functions of primary scenario mapping rules.</summary>
public static class PrimaryScenarioMappingRules
{
    public static PrimaryScenarioMappingValidator DefaultPrimaryScenarioMappingValidator()
    {
        return new PrimaryScenarioMappingValidator(
            new List<IPrimaryScenarioMappingRule>
            {
                new PrimaryScenarioMappingCodeLengthRule(80),
                new PrimaryScenarioMappingLabelLengthRule(1000),
                new PrimaryScenarioMappingCodePrefixRule("ACM"),
                new PrimaryScenarioMappingUnitLengthRule(16),
            });
    }
}
