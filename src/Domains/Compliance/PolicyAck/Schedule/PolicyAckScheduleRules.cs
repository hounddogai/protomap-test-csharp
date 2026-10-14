namespace Acme.Workforce.Compliance.PolicyAck.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack schedule must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckScheduleRule
{
    List<string> Check(PolicyAckSchedule item);
}

public sealed record PolicyAckScheduleRequiredCodeRule(string Message) : IPolicyAckScheduleRule
{
    public List<string> Check(PolicyAckSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckScheduleAllowedSegmentRule(List<string> Allowed) : IPolicyAckScheduleRule
{
    public List<string> Check(PolicyAckSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckScheduleCodeLengthRule(int MaxLength) : IPolicyAckScheduleRule
{
    public List<string> Check(PolicyAckSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckScheduleRequiredTitleRule(string Message) : IPolicyAckScheduleRule
{
    public List<string> Check(PolicyAckSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckScheduleTagsLimitRule(int MaxCount, List<string> Banned) : IPolicyAckScheduleRule
{
    public List<string> Check(PolicyAckSchedule item)
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

/// <summary>Runs every rule of a policy ack schedule.</summary>
public sealed record PolicyAckScheduleValidator(List<IPolicyAckScheduleRule> Rules)
{
    public List<string> Validate(PolicyAckSchedule item)
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

    public void RequireValid(PolicyAckSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckSchedule> items)
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

/// <summary>Functions of policy ack schedule rules.</summary>
public static class PolicyAckScheduleRules
{
    public static PolicyAckScheduleValidator DefaultPolicyAckScheduleValidator()
    {
        return new PolicyAckScheduleValidator(
            new List<IPolicyAckScheduleRule>
            {
                new PolicyAckScheduleRequiredCodeRule("policy ack schedule needs a value"),
                new PolicyAckScheduleAllowedSegmentRule(new List<string> { "default", "batched" }),
                new PolicyAckScheduleCodeLengthRule(32),
                new PolicyAckScheduleRequiredTitleRule("policy ack schedule needs this field"),
                new PolicyAckScheduleTagsLimitRule(120, new List<string> { "adaptive" }),
            });
    }
}
