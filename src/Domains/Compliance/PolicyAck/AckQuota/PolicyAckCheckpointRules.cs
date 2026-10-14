namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>A rule a policy ack checkpoint must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckCheckpointRule
{
    List<string> Check(PolicyAckCheckpoint item);
}

public sealed record PolicyAckCheckpointCodeLengthRule(int MaxLength) : IPolicyAckCheckpointRule
{
    public List<string> Check(PolicyAckCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckCheckpointRevisionRangeRule(int Min, int Max) : IPolicyAckCheckpointRule
{
    public List<string> Check(PolicyAckCheckpoint item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckCheckpointRequiredReviewerRule(string Message) : IPolicyAckCheckpointRule
{
    public List<string> Check(PolicyAckCheckpoint item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack checkpoint.</summary>
public sealed record PolicyAckCheckpointValidator(List<IPolicyAckCheckpointRule> Rules)
{
    public List<string> Validate(PolicyAckCheckpoint item)
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

    public void RequireValid(PolicyAckCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckCheckpoint> items)
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

/// <summary>Functions of policy ack checkpoint rules.</summary>
public static class PolicyAckCheckpointRules
{
    public static PolicyAckCheckpointValidator DefaultPolicyAckCheckpointValidator()
    {
        return new PolicyAckCheckpointValidator(
            new List<IPolicyAckCheckpointRule>
            {
                new PolicyAckCheckpointCodeLengthRule(3600),
                new PolicyAckCheckpointRevisionRangeRule(3600, 86400),
                new PolicyAckCheckpointRequiredReviewerRule("policy ack checkpoint needs this field"),
            });
    }
}
