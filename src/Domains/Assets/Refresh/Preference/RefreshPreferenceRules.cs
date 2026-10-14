namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh preference must pass. Check returns its problems, or none.</summary>
public interface IRefreshPreferenceRule
{
    List<string> Check(RefreshPreference item);
}

public sealed record RefreshPreferenceLimitRangeRule(int Min, int Max) : IRefreshPreferenceRule
{
    public List<string> Check(RefreshPreference item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshPreferenceAllowedCodeRule(List<string> Allowed) : IRefreshPreferenceRule
{
    public List<string> Check(RefreshPreference item)
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

public sealed record RefreshPreferenceRequiredBadgeRule(string Message) : IRefreshPreferenceRule
{
    public List<string> Check(RefreshPreference item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh preference.</summary>
public sealed record RefreshPreferenceValidator(List<IRefreshPreferenceRule> Rules)
{
    public List<string> Validate(RefreshPreference item)
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

    public void RequireValid(RefreshPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshPreference> items)
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

/// <summary>Functions of refresh preference rules.</summary>
public static class RefreshPreferenceRules
{
    public static RefreshPreferenceValidator DefaultRefreshPreferenceValidator()
    {
        return new RefreshPreferenceValidator(
            new List<IRefreshPreferenceRule>
            {
                new RefreshPreferenceLimitRangeRule(32, 255),
                new RefreshPreferenceAllowedCodeRule(new List<string> { "lenient" }),
                new RefreshPreferenceRequiredBadgeRule("refresh preference needs this field"),
            });
    }
}
