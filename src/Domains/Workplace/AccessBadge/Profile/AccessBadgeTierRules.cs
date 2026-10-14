namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge tier must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeTierRule
{
    List<string> Check(AccessBadgeTier item);
}

public sealed record AccessBadgeTierRequiredRegionRule(string Message) : IAccessBadgeTierRule
{
    public List<string> Check(AccessBadgeTier item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeTierActiveGuardRule(int Limit) : IAccessBadgeTierRule
{
    public List<string> Check(AccessBadgeTier item)
    {
        if (item.Active && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeTierAllowedRegionRule(List<string> Allowed) : IAccessBadgeTierRule
{
    public List<string> Check(AccessBadgeTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeTierLimitRangeRule(int Min, int Max) : IAccessBadgeTierRule
{
    public List<string> Check(AccessBadgeTier item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge tier.</summary>
public sealed record AccessBadgeTierValidator(List<IAccessBadgeTierRule> Rules)
{
    public List<string> Validate(AccessBadgeTier item)
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

    public void RequireValid(AccessBadgeTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeTier> items)
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

/// <summary>Functions of access badge tier rules.</summary>
public static class AccessBadgeTierRules
{
    public static AccessBadgeTierValidator DefaultAccessBadgeTierValidator()
    {
        return new AccessBadgeTierValidator(
            new List<IAccessBadgeTierRule>
            {
                new AccessBadgeTierRequiredRegionRule("access badge tier needs an entry"),
                new AccessBadgeTierActiveGuardRule(16),
                new AccessBadgeTierAllowedRegionRule(new List<string> { "weighted", "scheduled", "strict" }),
                new AccessBadgeTierLimitRangeRule(1000, 1000),
            });
    }
}
