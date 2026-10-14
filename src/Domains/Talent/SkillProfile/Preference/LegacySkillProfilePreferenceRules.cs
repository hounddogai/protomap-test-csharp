namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy skill profile preference must pass. Check returns its problems, or none.</summary>
public interface ILegacySkillProfilePreferenceRule
{
    List<string> Check(LegacySkillProfilePreference item);
}

public sealed record LegacySkillProfilePreferenceCodePrefixRule(string Prefix) : ILegacySkillProfilePreferenceRule
{
    public List<string> Check(LegacySkillProfilePreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySkillProfilePreferenceSummaryLengthRule(int MaxLength) : ILegacySkillProfilePreferenceRule
{
    public List<string> Check(LegacySkillProfilePreference item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySkillProfilePreferenceRequiredLocaleRule(string Message) : ILegacySkillProfilePreferenceRule
{
    public List<string> Check(LegacySkillProfilePreference item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacySkillProfilePreferenceAllowedLocaleRule(List<string> Allowed) : ILegacySkillProfilePreferenceRule
{
    public List<string> Check(LegacySkillProfilePreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is missing" };
        }
        return new List<string>();
    }
}

public sealed record LegacySkillProfilePreferenceNameLengthRule(int MaxLength) : ILegacySkillProfilePreferenceRule
{
    public List<string> Check(LegacySkillProfilePreference item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacySkillProfilePreferenceEndsAtRangeRule(int Min, int Max) : ILegacySkillProfilePreferenceRule
{
    public List<string> Check(LegacySkillProfilePreference item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a legacy skill profile preference.</summary>
public sealed record LegacySkillProfilePreferenceValidator(List<ILegacySkillProfilePreferenceRule> Rules)
{
    public List<string> Validate(LegacySkillProfilePreference item)
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

    public void RequireValid(LegacySkillProfilePreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacySkillProfilePreference> items)
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

/// <summary>Functions of legacy skill profile preference rules.</summary>
public static class LegacySkillProfilePreferenceRules
{
    public static LegacySkillProfilePreferenceValidator DefaultLegacySkillProfilePreferenceValidator()
    {
        return new LegacySkillProfilePreferenceValidator(
            new List<ILegacySkillProfilePreferenceRule>
            {
                new LegacySkillProfilePreferenceCodePrefixRule("ACM"),
                new LegacySkillProfilePreferenceSummaryLengthRule(80),
                new LegacySkillProfilePreferenceRequiredLocaleRule(
                    "legacy skill profile preference needs this field"),
                new LegacySkillProfilePreferenceAllowedLocaleRule(new List<string>()),
                new LegacySkillProfilePreferenceNameLengthRule(120),
                new LegacySkillProfilePreferenceEndsAtRangeRule(255, 255),
            });
    }
}
