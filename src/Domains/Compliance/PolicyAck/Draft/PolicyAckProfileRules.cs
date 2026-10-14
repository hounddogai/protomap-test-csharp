namespace Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>A rule a policy ack profile must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckProfileRule
{
    List<string> Check(PolicyAckProfile item);
}

public sealed record PolicyAckProfilePinnedGuardRule(double Limit) : IPolicyAckProfileRule
{
    public List<string> Check(PolicyAckProfile item)
    {
        if (item.Pinned && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckProfileCodePrefixRule(string Prefix) : IPolicyAckProfileRule
{
    public List<string> Check(PolicyAckProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckProfileDescriptionLengthRule(int MaxLength) : IPolicyAckProfileRule
{
    public List<string> Check(PolicyAckProfile item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckProfileTitleLengthRule(int MaxLength) : IPolicyAckProfileRule
{
    public List<string> Check(PolicyAckProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack profile.</summary>
public sealed record PolicyAckProfileValidator(List<IPolicyAckProfileRule> Rules)
{
    public List<string> Validate(PolicyAckProfile item)
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

    public void RequireValid(PolicyAckProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckProfile> items)
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

/// <summary>Functions of policy ack profile rules.</summary>
public static class PolicyAckProfileRules
{
    public static PolicyAckProfileValidator DefaultPolicyAckProfileValidator()
    {
        return new PolicyAckProfileValidator(
            new List<IPolicyAckProfileRule>
            {
                new PolicyAckProfilePinnedGuardRule(7.1),
                new PolicyAckProfileCodePrefixRule("Q"),
                new PolicyAckProfileDescriptionLengthRule(16),
                new PolicyAckProfileTitleLengthRule(64),
            });
    }
}
