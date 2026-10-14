namespace Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>A rule a strict mobility override must pass. Check returns its problems, or none.</summary>
public interface IStrictMobilityOverrideRule
{
    List<string> Check(StrictMobilityOverride item);
}

public sealed record StrictMobilityOverrideCodePrefixRule(string Prefix) : IStrictMobilityOverrideRule
{
    public List<string> Check(StrictMobilityOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictMobilityOverrideCurrencyLengthRule(int MaxLength) : IStrictMobilityOverrideRule
{
    public List<string> Check(StrictMobilityOverride item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictMobilityOverrideCodeLengthRule(int MaxLength) : IStrictMobilityOverrideRule
{
    public List<string> Check(StrictMobilityOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict mobility override.</summary>
public sealed record StrictMobilityOverrideValidator(List<IStrictMobilityOverrideRule> Rules)
{
    public List<string> Validate(StrictMobilityOverride item)
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

    public void RequireValid(StrictMobilityOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictMobilityOverride> items)
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

/// <summary>Functions of strict mobility override rules.</summary>
public static class StrictMobilityOverrideRules
{
    public static StrictMobilityOverrideValidator DefaultStrictMobilityOverrideValidator()
    {
        return new StrictMobilityOverrideValidator(
            new List<IStrictMobilityOverrideRule>
            {
                new StrictMobilityOverrideCodePrefixRule("X"),
                new StrictMobilityOverrideCurrencyLengthRule(120),
                new StrictMobilityOverrideCodeLengthRule(3600),
            });
    }
}
