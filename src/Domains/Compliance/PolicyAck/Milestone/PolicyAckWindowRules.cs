namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

/// <summary>A rule a policy ack window must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckWindowRule
{
    List<string> Check(PolicyAckWindow item);
}

public sealed record PolicyAckWindowCodePrefixRule(string Prefix) : IPolicyAckWindowRule
{
    public List<string> Check(PolicyAckWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckWindowTagsLimitRule(int MaxCount, List<string> Banned) : IPolicyAckWindowRule
{
    public List<string> Check(PolicyAckWindow item)
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

public sealed record PolicyAckWindowWindowDaysRangeRule(int Min, int Max) : IPolicyAckWindowRule
{
    public List<string> Check(PolicyAckWindow item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a policy ack window.</summary>
public sealed record PolicyAckWindowValidator(List<IPolicyAckWindowRule> Rules)
{
    public List<string> Validate(PolicyAckWindow item)
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

    public void RequireValid(PolicyAckWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckWindow> items)
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

/// <summary>Functions of policy ack window rules.</summary>
public static class PolicyAckWindowRules
{
    public static PolicyAckWindowValidator DefaultPolicyAckWindowValidator()
    {
        return new PolicyAckWindowValidator(
            new List<IPolicyAckWindowRule>
            {
                new PolicyAckWindowCodePrefixRule("ACM"),
                new PolicyAckWindowTagsLimitRule(80, new List<string> { "pending" }),
                new PolicyAckWindowWindowDaysRangeRule(3600, 86400),
            });
    }
}
