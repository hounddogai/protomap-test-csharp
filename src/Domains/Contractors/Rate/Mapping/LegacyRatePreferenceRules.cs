namespace Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>A rule a legacy rate preference must pass. Check returns its problems, or none.</summary>
public interface ILegacyRatePreferenceRule
{
    List<string> Check(LegacyRatePreference item);
}

public sealed record LegacyRatePreferenceCeilingRangeRule(double Min, double Max) : ILegacyRatePreferenceRule
{
    public List<string> Check(LegacyRatePreference item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRatePreferenceCodePrefixRule(string Prefix) : ILegacyRatePreferenceRule
{
    public List<string> Check(LegacyRatePreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRatePreferenceCodeLengthRule(int MaxLength) : ILegacyRatePreferenceRule
{
    public List<string> Check(LegacyRatePreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy rate preference.</summary>
public sealed record LegacyRatePreferenceValidator(List<ILegacyRatePreferenceRule> Rules)
{
    public List<string> Validate(LegacyRatePreference item)
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

    public void RequireValid(LegacyRatePreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRatePreference> items)
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

/// <summary>Functions of legacy rate preference rules.</summary>
public static class LegacyRatePreferenceRules
{
    public static LegacyRatePreferenceValidator DefaultLegacyRatePreferenceValidator()
    {
        return new LegacyRatePreferenceValidator(
            new List<ILegacyRatePreferenceRule>
            {
                new LegacyRatePreferenceCeilingRangeRule(0.9, 1386.4),
                new LegacyRatePreferenceCodePrefixRule("OPS"),
                new LegacyRatePreferenceCodeLengthRule(120),
            });
    }
}
