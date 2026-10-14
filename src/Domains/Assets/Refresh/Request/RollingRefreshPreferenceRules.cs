namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling refresh preference must pass. Check returns its problems, or none.</summary>
public interface IRollingRefreshPreferenceRule
{
    List<string> Check(RollingRefreshPreference item);
}

public sealed record RollingRefreshPreferenceTagsLimitRule(int MaxCount, List<string> Banned) : IRollingRefreshPreferenceRule
{
    public List<string> Check(RollingRefreshPreference item)
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

public sealed record RollingRefreshPreferenceDurationMinutesRangeRule(int Min, int Max) : IRollingRefreshPreferenceRule
{
    public List<string> Check(RollingRefreshPreference item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRefreshPreferenceAllowedLabelRule(List<string> Allowed) : IRollingRefreshPreferenceRule
{
    public List<string> Check(RollingRefreshPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling refresh preference.</summary>
public sealed record RollingRefreshPreferenceValidator(List<IRollingRefreshPreferenceRule> Rules)
{
    public List<string> Validate(RollingRefreshPreference item)
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

    public void RequireValid(RollingRefreshPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRefreshPreference> items)
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

/// <summary>Functions of rolling refresh preference rules.</summary>
public static class RollingRefreshPreferenceRules
{
    public static RollingRefreshPreferenceValidator DefaultRollingRefreshPreferenceValidator()
    {
        return new RollingRefreshPreferenceValidator(
            new List<IRollingRefreshPreferenceRule>
            {
                new RollingRefreshPreferenceTagsLimitRule(64, new List<string> { "default", "global", "strict" }),
                new RollingRefreshPreferenceDurationMinutesRangeRule(16, 80),
                new RollingRefreshPreferenceAllowedLabelRule(new List<string> { "fallback", "lenient" }),
            });
    }
}
