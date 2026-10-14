namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile preference must pass. Check returns its problems, or none.</summary>
public interface ISkillProfilePreferenceRule
{
    List<string> Check(SkillProfilePreference item);
}

public sealed record SkillProfilePreferenceMarginRangeRule(double Min, double Max) : ISkillProfilePreferenceRule
{
    public List<string> Check(SkillProfilePreference item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfilePreferenceCodePrefixRule(string Prefix) : ISkillProfilePreferenceRule
{
    public List<string> Check(SkillProfilePreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfilePreferenceLabelLengthRule(int MaxLength) : ISkillProfilePreferenceRule
{
    public List<string> Check(SkillProfilePreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfilePreferenceCountryLengthRule(int MaxLength) : ISkillProfilePreferenceRule
{
    public List<string> Check(SkillProfilePreference item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfilePreferenceAllowedCodeRule(List<string> Allowed) : ISkillProfilePreferenceRule
{
    public List<string> Check(SkillProfilePreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile preference.</summary>
public sealed record SkillProfilePreferenceValidator(List<ISkillProfilePreferenceRule> Rules)
{
    public List<string> Validate(SkillProfilePreference item)
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

    public void RequireValid(SkillProfilePreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfilePreference> items)
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

/// <summary>Functions of skill profile preference rules.</summary>
public static class SkillProfilePreferenceRules
{
    public static SkillProfilePreferenceValidator DefaultSkillProfilePreferenceValidator()
    {
        return new SkillProfilePreferenceValidator(
            new List<ISkillProfilePreferenceRule>
            {
                new SkillProfilePreferenceMarginRangeRule(3.3, 2835.0),
                new SkillProfilePreferenceCodePrefixRule("ACM"),
                new SkillProfilePreferenceLabelLengthRule(1000),
                new SkillProfilePreferenceCountryLengthRule(86400),
                new SkillProfilePreferenceAllowedCodeRule(new List<string> { "legacy", "regional", "global" }),
            });
    }
}
