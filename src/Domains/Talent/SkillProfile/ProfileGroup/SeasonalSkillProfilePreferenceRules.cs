namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>A rule a seasonal skill profile preference must pass. Check returns its problems, or none.</summary>
public interface ISeasonalSkillProfilePreferenceRule
{
    List<string> Check(SeasonalSkillProfilePreference item);
}

public sealed record SeasonalSkillProfilePreferenceRequiredCountryRule(string Message) : ISeasonalSkillProfilePreferenceRule
{
    public List<string> Check(SeasonalSkillProfilePreference item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSkillProfilePreferenceCodePrefixRule(string Prefix) : ISeasonalSkillProfilePreferenceRule
{
    public List<string> Check(SeasonalSkillProfilePreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSkillProfilePreferenceRequiredCodeRule(string Message) : ISeasonalSkillProfilePreferenceRule
{
    public List<string> Check(SeasonalSkillProfilePreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSkillProfilePreferenceLocaleLengthRule(int MaxLength) : ISeasonalSkillProfilePreferenceRule
{
    public List<string> Check(SeasonalSkillProfilePreference item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSkillProfilePreferenceCountryLengthRule(int MaxLength) : ISeasonalSkillProfilePreferenceRule
{
    public List<string> Check(SeasonalSkillProfilePreference item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSkillProfilePreferencePinnedGuardRule(int Limit) : ISeasonalSkillProfilePreferenceRule
{
    public List<string> Check(SeasonalSkillProfilePreference item)
    {
        if (item.Pinned && item.TenureMonths > Limit)
        {
            return new List<string> { $"tenure months {item.TenureMonths} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal skill profile preference.</summary>
public sealed record SeasonalSkillProfilePreferenceValidator(List<ISeasonalSkillProfilePreferenceRule> Rules)
{
    public List<string> Validate(SeasonalSkillProfilePreference item)
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

    public void RequireValid(SeasonalSkillProfilePreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalSkillProfilePreference> items)
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

/// <summary>Functions of seasonal skill profile preference rules.</summary>
public static class SeasonalSkillProfilePreferenceRules
{
    public static SeasonalSkillProfilePreferenceValidator DefaultSeasonalSkillProfilePreferenceValidator()
    {
        return new SeasonalSkillProfilePreferenceValidator(
            new List<ISeasonalSkillProfilePreferenceRule>
            {
                new SeasonalSkillProfilePreferenceRequiredCountryRule(
                    "seasonal skill profile preference needs an entry"),
                new SeasonalSkillProfilePreferenceCodePrefixRule("OPS"),
                new SeasonalSkillProfilePreferenceRequiredCodeRule(
                    "seasonal skill profile preference needs this field"),
                new SeasonalSkillProfilePreferenceLocaleLengthRule(120),
                new SeasonalSkillProfilePreferenceCountryLengthRule(32),
                new SeasonalSkillProfilePreferencePinnedGuardRule(1000),
            });
    }
}
