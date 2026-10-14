namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional scenario batch must pass. Check returns its problems, or none.</summary>
public interface IRegionalScenarioBatchRule
{
    List<string> Check(RegionalScenarioBatch item);
}

public sealed record RegionalScenarioBatchCurrencyLengthRule(int MaxLength) : IRegionalScenarioBatchRule
{
    public List<string> Check(RegionalScenarioBatch item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalScenarioBatchAllowedCodeRule(List<string> Allowed) : IRegionalScenarioBatchRule
{
    public List<string> Check(RegionalScenarioBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RegionalScenarioBatchCodeLengthRule(int MaxLength) : IRegionalScenarioBatchRule
{
    public List<string> Check(RegionalScenarioBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalScenarioBatchCodePrefixRule(string Prefix) : IRegionalScenarioBatchRule
{
    public List<string> Check(RegionalScenarioBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional scenario batch.</summary>
public sealed record RegionalScenarioBatchValidator(List<IRegionalScenarioBatchRule> Rules)
{
    public List<string> Validate(RegionalScenarioBatch item)
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

    public void RequireValid(RegionalScenarioBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalScenarioBatch> items)
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

/// <summary>Functions of regional scenario batch rules.</summary>
public static class RegionalScenarioBatchRules
{
    public static RegionalScenarioBatchValidator DefaultRegionalScenarioBatchValidator()
    {
        return new RegionalScenarioBatchValidator(
            new List<IRegionalScenarioBatchRule>
            {
                new RegionalScenarioBatchCurrencyLengthRule(80),
                new RegionalScenarioBatchAllowedCodeRule(new List<string>()),
                new RegionalScenarioBatchCodeLengthRule(16),
                new RegionalScenarioBatchCodePrefixRule("OPS"),
            });
    }
}
