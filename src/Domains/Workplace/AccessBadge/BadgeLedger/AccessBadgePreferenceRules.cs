namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>A rule a access badge preference must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgePreferenceRule
{
    List<string> Check(AccessBadgePreference item);
}

public sealed record AccessBadgePreferenceTitleLengthRule(int MaxLength) : IAccessBadgePreferenceRule
{
    public List<string> Check(AccessBadgePreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePreferenceCodeLengthRule(int MaxLength) : IAccessBadgePreferenceRule
{
    public List<string> Check(AccessBadgePreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePreferenceSummaryLengthRule(int MaxLength) : IAccessBadgePreferenceRule
{
    public List<string> Check(AccessBadgePreference item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePreferenceRequiredCodeRule(string Message) : IAccessBadgePreferenceRule
{
    public List<string> Check(AccessBadgePreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePreferenceCeilingRangeRule(double Min, double Max) : IAccessBadgePreferenceRule
{
    public List<string> Check(AccessBadgePreference item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePreferenceRequiredTitleRule(string Message) : IAccessBadgePreferenceRule
{
    public List<string> Check(AccessBadgePreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge preference.</summary>
public sealed record AccessBadgePreferenceValidator(List<IAccessBadgePreferenceRule> Rules)
{
    public List<string> Validate(AccessBadgePreference item)
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

    public void RequireValid(AccessBadgePreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgePreference> items)
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

/// <summary>Functions of access badge preference rules.</summary>
public static class AccessBadgePreferenceRules
{
    public static AccessBadgePreferenceValidator DefaultAccessBadgePreferenceValidator()
    {
        return new AccessBadgePreferenceValidator(
            new List<IAccessBadgePreferenceRule>
            {
                new AccessBadgePreferenceTitleLengthRule(64),
                new AccessBadgePreferenceCodeLengthRule(255),
                new AccessBadgePreferenceSummaryLengthRule(3600),
                new AccessBadgePreferenceRequiredCodeRule("access badge preference needs an entry"),
                new AccessBadgePreferenceCeilingRangeRule(9.3, 3261.6),
                new AccessBadgePreferenceRequiredTitleRule("access badge preference needs this field"),
            });
    }
}
