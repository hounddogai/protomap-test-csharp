namespace Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>A rule a scheduled access badge target must pass. Check returns its problems, or none.</summary>
public interface IScheduledAccessBadgeTargetRule
{
    List<string> Check(ScheduledAccessBadgeTarget item);
}

public sealed record ScheduledAccessBadgeTargetOwnerIdLengthRule(int MaxLength) : IScheduledAccessBadgeTargetRule
{
    public List<string> Check(ScheduledAccessBadgeTarget item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccessBadgeTargetRequiredNameRule(string Message) : IScheduledAccessBadgeTargetRule
{
    public List<string> Check(ScheduledAccessBadgeTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccessBadgeTargetRequiredOwnerIdRule(string Message) : IScheduledAccessBadgeTargetRule
{
    public List<string> Check(ScheduledAccessBadgeTarget item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAccessBadgeTargetRequiredCodeRule(string Message) : IScheduledAccessBadgeTargetRule
{
    public List<string> Check(ScheduledAccessBadgeTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled access badge target.</summary>
public sealed record ScheduledAccessBadgeTargetValidator(List<IScheduledAccessBadgeTargetRule> Rules)
{
    public List<string> Validate(ScheduledAccessBadgeTarget item)
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

    public void RequireValid(ScheduledAccessBadgeTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAccessBadgeTarget> items)
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

/// <summary>Functions of scheduled access badge target rules.</summary>
public static class ScheduledAccessBadgeTargetRules
{
    public static ScheduledAccessBadgeTargetValidator DefaultScheduledAccessBadgeTargetValidator()
    {
        return new ScheduledAccessBadgeTargetValidator(
            new List<IScheduledAccessBadgeTargetRule>
            {
                new ScheduledAccessBadgeTargetOwnerIdLengthRule(120),
                new ScheduledAccessBadgeTargetRequiredNameRule("scheduled access badge target needs this field"),
                new ScheduledAccessBadgeTargetRequiredOwnerIdRule("scheduled access badge target needs a value"),
                new ScheduledAccessBadgeTargetRequiredCodeRule("scheduled access badge target needs a value"),
            });
    }
}
