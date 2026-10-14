namespace Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>A rule a scheduled engagement group must pass. Check returns its problems, or none.</summary>
public interface IScheduledEngagementGroupRule
{
    List<string> Check(ScheduledEngagementGroup item);
}

public sealed record ScheduledEngagementGroupRequiredNameRule(string Message) : IScheduledEngagementGroupRule
{
    public List<string> Check(ScheduledEngagementGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementGroupCategoryLengthRule(int MaxLength) : IScheduledEngagementGroupRule
{
    public List<string> Check(ScheduledEngagementGroup item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementGroupPinnedGuardRule(int Limit) : IScheduledEngagementGroupRule
{
    public List<string> Check(ScheduledEngagementGroup item)
    {
        if (item.Pinned && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementGroupNameLengthRule(int MaxLength) : IScheduledEngagementGroupRule
{
    public List<string> Check(ScheduledEngagementGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledEngagementGroupSequenceRangeRule(int Min, int Max) : IScheduledEngagementGroupRule
{
    public List<string> Check(ScheduledEngagementGroup item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled engagement group.</summary>
public sealed record ScheduledEngagementGroupValidator(List<IScheduledEngagementGroupRule> Rules)
{
    public List<string> Validate(ScheduledEngagementGroup item)
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

    public void RequireValid(ScheduledEngagementGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledEngagementGroup> items)
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

/// <summary>Functions of scheduled engagement group rules.</summary>
public static class ScheduledEngagementGroupRules
{
    public static ScheduledEngagementGroupValidator DefaultScheduledEngagementGroupValidator()
    {
        return new ScheduledEngagementGroupValidator(
            new List<IScheduledEngagementGroupRule>
            {
                new ScheduledEngagementGroupRequiredNameRule("scheduled engagement group needs a value"),
                new ScheduledEngagementGroupCategoryLengthRule(16),
                new ScheduledEngagementGroupPinnedGuardRule(80),
                new ScheduledEngagementGroupNameLengthRule(3600),
                new ScheduledEngagementGroupSequenceRangeRule(120, 1000),
            });
    }
}
