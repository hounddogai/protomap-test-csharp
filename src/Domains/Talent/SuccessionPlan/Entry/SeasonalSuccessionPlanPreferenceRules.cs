namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>A rule a seasonal succession plan preference must pass. Check returns its problems, or none.</summary>
public interface ISeasonalSuccessionPlanPreferenceRule
{
    List<string> Check(SeasonalSuccessionPlanPreference item);
}

public sealed record SeasonalSuccessionPlanPreferenceCodeLengthRule(int MaxLength) : ISeasonalSuccessionPlanPreferenceRule
{
    public List<string> Check(SeasonalSuccessionPlanPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSuccessionPlanPreferencePhoneLengthRule(int MaxLength) : ISeasonalSuccessionPlanPreferenceRule
{
    public List<string> Check(SeasonalSuccessionPlanPreference item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSuccessionPlanPreferenceLocaleLengthRule(int MaxLength) : ISeasonalSuccessionPlanPreferenceRule
{
    public List<string> Check(SeasonalSuccessionPlanPreference item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSuccessionPlanPreferenceCodePrefixRule(string Prefix) : ISeasonalSuccessionPlanPreferenceRule
{
    public List<string> Check(SeasonalSuccessionPlanPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal succession plan preference.</summary>
public sealed record SeasonalSuccessionPlanPreferenceValidator(List<ISeasonalSuccessionPlanPreferenceRule> Rules)
{
    public List<string> Validate(SeasonalSuccessionPlanPreference item)
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

    public void RequireValid(SeasonalSuccessionPlanPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalSuccessionPlanPreference> items)
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

/// <summary>Functions of seasonal succession plan preference rules.</summary>
public static class SeasonalSuccessionPlanPreferenceRules
{
    public static SeasonalSuccessionPlanPreferenceValidator DefaultSeasonalSuccessionPlanPreferenceValidator()
    {
        return new SeasonalSuccessionPlanPreferenceValidator(
            new List<ISeasonalSuccessionPlanPreferenceRule>
            {
                new SeasonalSuccessionPlanPreferenceCodeLengthRule(80),
                new SeasonalSuccessionPlanPreferencePhoneLengthRule(32),
                new SeasonalSuccessionPlanPreferenceLocaleLengthRule(120),
                new SeasonalSuccessionPlanPreferenceCodePrefixRule("OPS"),
            });
    }
}
