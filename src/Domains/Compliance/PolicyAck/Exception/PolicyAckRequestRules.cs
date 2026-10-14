namespace Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>A rule a policy ack request must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckRequestRule
{
    List<string> Check(PolicyAckRequest item);
}

public sealed record PolicyAckRequestRevisionRangeRule(int Min, int Max) : IPolicyAckRequestRule
{
    public List<string> Check(PolicyAckRequest item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckRequestRequiredTitleRule(string Message) : IPolicyAckRequestRule
{
    public List<string> Check(PolicyAckRequest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckRequestExpiresAtRangeRule(int Min, int Max) : IPolicyAckRequestRule
{
    public List<string> Check(PolicyAckRequest item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyAckRequestTitleLengthRule(int MaxLength) : IPolicyAckRequestRule
{
    public List<string> Check(PolicyAckRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack request.</summary>
public sealed record PolicyAckRequestValidator(List<IPolicyAckRequestRule> Rules)
{
    public List<string> Validate(PolicyAckRequest item)
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

    public void RequireValid(PolicyAckRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckRequest> items)
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

/// <summary>Functions of policy ack request rules.</summary>
public static class PolicyAckRequestRules
{
    public static PolicyAckRequestValidator DefaultPolicyAckRequestValidator()
    {
        return new PolicyAckRequestValidator(
            new List<IPolicyAckRequestRule>
            {
                new PolicyAckRequestRevisionRangeRule(32, 64),
                new PolicyAckRequestRequiredTitleRule("policy ack request needs this field"),
                new PolicyAckRequestExpiresAtRangeRule(32, 64),
                new PolicyAckRequestTitleLengthRule(64),
            });
    }
}
