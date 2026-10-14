namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge threshold must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeThresholdRule
{
    List<string> Check(AccessBadgeThreshold item);
}

public sealed record AccessBadgeThresholdAllowedChannelRule(List<string> Allowed) : IAccessBadgeThresholdRule
{
    public List<string> Check(AccessBadgeThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeThresholdArchivedGuardRule(double Limit) : IAccessBadgeThresholdRule
{
    public List<string> Check(AccessBadgeThreshold item)
    {
        if (item.Archived && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeThresholdCodeLengthRule(int MaxLength) : IAccessBadgeThresholdRule
{
    public List<string> Check(AccessBadgeThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeThresholdNameLengthRule(int MaxLength) : IAccessBadgeThresholdRule
{
    public List<string> Check(AccessBadgeThreshold item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge threshold.</summary>
public sealed record AccessBadgeThresholdValidator(List<IAccessBadgeThresholdRule> Rules)
{
    public List<string> Validate(AccessBadgeThreshold item)
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

    public void RequireValid(AccessBadgeThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeThreshold> items)
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

/// <summary>Functions of access badge threshold rules.</summary>
public static class AccessBadgeThresholdRules
{
    public static AccessBadgeThresholdValidator DefaultAccessBadgeThresholdValidator()
    {
        return new AccessBadgeThresholdValidator(
            new List<IAccessBadgeThresholdRule>
            {
                new AccessBadgeThresholdAllowedChannelRule(new List<string> { "batched", "adaptive" }),
                new AccessBadgeThresholdArchivedGuardRule(7.6),
                new AccessBadgeThresholdCodeLengthRule(32),
                new AccessBadgeThresholdNameLengthRule(1000),
            });
    }
}
