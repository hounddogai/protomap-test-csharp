namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge profile must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeProfileRule
{
    List<string> Check(AccessBadgeProfile item);
}

public sealed record AccessBadgeProfileGraceDaysRangeRule(int Min, int Max) : IAccessBadgeProfileRule
{
    public List<string> Check(AccessBadgeProfile item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccessBadgeProfileAllowedCodeRule(List<string> Allowed) : IAccessBadgeProfileRule
{
    public List<string> Check(AccessBadgeProfile item)
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

public sealed record AccessBadgeProfileNotesLengthRule(int MaxLength) : IAccessBadgeProfileRule
{
    public List<string> Check(AccessBadgeProfile item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeProfileCodeLengthRule(int MaxLength) : IAccessBadgeProfileRule
{
    public List<string> Check(AccessBadgeProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge profile.</summary>
public sealed record AccessBadgeProfileValidator(List<IAccessBadgeProfileRule> Rules)
{
    public List<string> Validate(AccessBadgeProfile item)
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

    public void RequireValid(AccessBadgeProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeProfile> items)
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

/// <summary>Functions of access badge profile rules.</summary>
public static class AccessBadgeProfileRules
{
    public static AccessBadgeProfileValidator DefaultAccessBadgeProfileValidator()
    {
        return new AccessBadgeProfileValidator(
            new List<IAccessBadgeProfileRule>
            {
                new AccessBadgeProfileGraceDaysRangeRule(1000, 86400),
                new AccessBadgeProfileAllowedCodeRule(new List<string> { "strict", "regional" }),
                new AccessBadgeProfileNotesLengthRule(86400),
                new AccessBadgeProfileCodeLengthRule(64),
            });
    }
}
