namespace Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>A rule a primary adjustment preference must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAdjustmentPreferenceRule
{
    List<string> Check(PrimaryAdjustmentPreference item);
}

public sealed record PrimaryAdjustmentPreferenceRequiredSkillRule(string Message) : IPrimaryAdjustmentPreferenceRule
{
    public List<string> Check(PrimaryAdjustmentPreference item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAdjustmentPreferenceTagsLimitRule(int MaxCount, List<string> Banned) : IPrimaryAdjustmentPreferenceRule
{
    public List<string> Check(PrimaryAdjustmentPreference item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record PrimaryAdjustmentPreferenceRequiredLabelRule(string Message) : IPrimaryAdjustmentPreferenceRule
{
    public List<string> Check(PrimaryAdjustmentPreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary adjustment preference.</summary>
public sealed record PrimaryAdjustmentPreferenceValidator(List<IPrimaryAdjustmentPreferenceRule> Rules)
{
    public List<string> Validate(PrimaryAdjustmentPreference item)
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

    public void RequireValid(PrimaryAdjustmentPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAdjustmentPreference> items)
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

/// <summary>Functions of primary adjustment preference rules.</summary>
public static class PrimaryAdjustmentPreferenceRules
{
    public static PrimaryAdjustmentPreferenceValidator DefaultPrimaryAdjustmentPreferenceValidator()
    {
        return new PrimaryAdjustmentPreferenceValidator(
            new List<IPrimaryAdjustmentPreferenceRule>
            {
                new PrimaryAdjustmentPreferenceRequiredSkillRule("primary adjustment preference needs this field"),
                new PrimaryAdjustmentPreferenceTagsLimitRule(
                    64,
                    new List<string> { "adaptive", "scheduled", "primary" }),
                new PrimaryAdjustmentPreferenceRequiredLabelRule("primary adjustment preference needs this field"),
            });
    }
}
