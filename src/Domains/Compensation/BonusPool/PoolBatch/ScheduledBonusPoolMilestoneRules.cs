namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled bonus pool milestone must pass. Check returns its problems, or none.</summary>
public interface IScheduledBonusPoolMilestoneRule
{
    List<string> Check(ScheduledBonusPoolMilestone item);
}

public sealed record ScheduledBonusPoolMilestoneRequiredNameRule(string Message) : IScheduledBonusPoolMilestoneRule
{
    public List<string> Check(ScheduledBonusPoolMilestone item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBonusPoolMilestoneAllowedNameRule(List<string> Allowed) : IScheduledBonusPoolMilestoneRule
{
    public List<string> Check(ScheduledBonusPoolMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBonusPoolMilestoneNameLengthRule(int MaxLength) : IScheduledBonusPoolMilestoneRule
{
    public List<string> Check(ScheduledBonusPoolMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled bonus pool milestone.</summary>
public sealed record ScheduledBonusPoolMilestoneValidator(List<IScheduledBonusPoolMilestoneRule> Rules)
{
    public List<string> Validate(ScheduledBonusPoolMilestone item)
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

    public void RequireValid(ScheduledBonusPoolMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBonusPoolMilestone> items)
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

/// <summary>Functions of scheduled bonus pool milestone rules.</summary>
public static class ScheduledBonusPoolMilestoneRules
{
    public static ScheduledBonusPoolMilestoneValidator DefaultScheduledBonusPoolMilestoneValidator()
    {
        return new ScheduledBonusPoolMilestoneValidator(
            new List<IScheduledBonusPoolMilestoneRule>
            {
                new ScheduledBonusPoolMilestoneRequiredNameRule("scheduled bonus pool milestone needs a value"),
                new ScheduledBonusPoolMilestoneAllowedNameRule(new List<string> { "weighted", "primary" }),
                new ScheduledBonusPoolMilestoneNameLengthRule(255),
            });
    }
}
