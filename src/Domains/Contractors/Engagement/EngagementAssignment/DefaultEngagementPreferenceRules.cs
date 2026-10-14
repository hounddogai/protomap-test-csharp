namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>A rule a default engagement preference must pass. Check returns its problems, or none.</summary>
public interface IDefaultEngagementPreferenceRule
{
    List<string> Check(DefaultEngagementPreference item);
}

public sealed record DefaultEngagementPreferenceNameLengthRule(int MaxLength) : IDefaultEngagementPreferenceRule
{
    public List<string> Check(DefaultEngagementPreference item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementPreferenceLabelLengthRule(int MaxLength) : IDefaultEngagementPreferenceRule
{
    public List<string> Check(DefaultEngagementPreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementPreferenceLimitRangeRule(int Min, int Max) : IDefaultEngagementPreferenceRule
{
    public List<string> Check(DefaultEngagementPreference item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultEngagementPreferenceRequiredCodeRule(string Message) : IDefaultEngagementPreferenceRule
{
    public List<string> Check(DefaultEngagementPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementPreferencePinnedGuardRule(int Limit) : IDefaultEngagementPreferenceRule
{
    public List<string> Check(DefaultEngagementPreference item)
    {
        if (item.Pinned && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record DefaultEngagementPreferenceCodePrefixRule(string Prefix) : IDefaultEngagementPreferenceRule
{
    public List<string> Check(DefaultEngagementPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default engagement preference.</summary>
public sealed record DefaultEngagementPreferenceValidator(List<IDefaultEngagementPreferenceRule> Rules)
{
    public List<string> Validate(DefaultEngagementPreference item)
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

    public void RequireValid(DefaultEngagementPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultEngagementPreference> items)
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

/// <summary>Functions of default engagement preference rules.</summary>
public static class DefaultEngagementPreferenceRules
{
    public static DefaultEngagementPreferenceValidator DefaultDefaultEngagementPreferenceValidator()
    {
        return new DefaultEngagementPreferenceValidator(
            new List<IDefaultEngagementPreferenceRule>
            {
                new DefaultEngagementPreferenceNameLengthRule(1000),
                new DefaultEngagementPreferenceLabelLengthRule(255),
                new DefaultEngagementPreferenceLimitRangeRule(80, 86400),
                new DefaultEngagementPreferenceRequiredCodeRule("default engagement preference needs a value"),
                new DefaultEngagementPreferencePinnedGuardRule(3600),
                new DefaultEngagementPreferenceCodePrefixRule("X"),
            });
    }
}
