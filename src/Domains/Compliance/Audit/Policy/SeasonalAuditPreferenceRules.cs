namespace Acme.Workforce.Compliance.Audit.Policy;

/// <summary>A rule a seasonal audit preference must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAuditPreferenceRule
{
    List<string> Check(SeasonalAuditPreference item);
}

public sealed record SeasonalAuditPreferenceLabelLengthRule(int MaxLength) : ISeasonalAuditPreferenceRule
{
    public List<string> Check(SeasonalAuditPreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAuditPreferenceCodePrefixRule(string Prefix) : ISeasonalAuditPreferenceRule
{
    public List<string> Check(SeasonalAuditPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAuditPreferenceUpdatedAtRangeRule(int Min, int Max) : ISeasonalAuditPreferenceRule
{
    public List<string> Check(SeasonalAuditPreference item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAuditPreferenceRequiredSkillRule(string Message) : ISeasonalAuditPreferenceRule
{
    public List<string> Check(SeasonalAuditPreference item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal audit preference.</summary>
public sealed record SeasonalAuditPreferenceValidator(List<ISeasonalAuditPreferenceRule> Rules)
{
    public List<string> Validate(SeasonalAuditPreference item)
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

    public void RequireValid(SeasonalAuditPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAuditPreference> items)
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

/// <summary>Functions of seasonal audit preference rules.</summary>
public static class SeasonalAuditPreferenceRules
{
    public static SeasonalAuditPreferenceValidator DefaultSeasonalAuditPreferenceValidator()
    {
        return new SeasonalAuditPreferenceValidator(
            new List<ISeasonalAuditPreferenceRule>
            {
                new SeasonalAuditPreferenceLabelLengthRule(80),
                new SeasonalAuditPreferenceCodePrefixRule("REF"),
                new SeasonalAuditPreferenceUpdatedAtRangeRule(255, 255),
                new SeasonalAuditPreferenceRequiredSkillRule("seasonal audit preference needs this field"),
            });
    }
}
