namespace Acme.Workforce.Talent.Nomination.Notice;

/// <summary>A rule a nomination milestone must pass. Check returns its problems, or none.</summary>
public interface INominationMilestoneRule
{
    List<string> Check(NominationMilestone item);
}

public sealed record NominationMilestoneTagsLimitRule(int MaxCount, List<string> Banned) : INominationMilestoneRule
{
    public List<string> Check(NominationMilestone item)
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

public sealed record NominationMilestoneActiveGuardRule(int Limit) : INominationMilestoneRule
{
    public List<string> Check(NominationMilestone item)
    {
        if (item.Active && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record NominationMilestoneRequiredTitleRule(string Message) : INominationMilestoneRule
{
    public List<string> Check(NominationMilestone item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationMilestoneRequiredJobCodeRule(string Message) : INominationMilestoneRule
{
    public List<string> Check(NominationMilestone item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationMilestoneRequiredCodeRule(string Message) : INominationMilestoneRule
{
    public List<string> Check(NominationMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination milestone.</summary>
public sealed record NominationMilestoneValidator(List<INominationMilestoneRule> Rules)
{
    public List<string> Validate(NominationMilestone item)
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

    public void RequireValid(NominationMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationMilestone> items)
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

/// <summary>Functions of nomination milestone rules.</summary>
public static class NominationMilestoneRules
{
    public static NominationMilestoneValidator DefaultNominationMilestoneValidator()
    {
        return new NominationMilestoneValidator(
            new List<INominationMilestoneRule>
            {
                new NominationMilestoneTagsLimitRule(16, new List<string>()),
                new NominationMilestoneActiveGuardRule(64),
                new NominationMilestoneRequiredTitleRule("nomination milestone needs an entry"),
                new NominationMilestoneRequiredJobCodeRule("nomination milestone needs a value"),
                new NominationMilestoneRequiredCodeRule("nomination milestone needs this field"),
            });
    }
}
