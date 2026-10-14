namespace Acme.Workforce.Compliance.PolicyAck.Ledger;

/// <summary>A rule a rolling policy ack milestone must pass. Check returns its problems, or none.</summary>
public interface IRollingPolicyAckMilestoneRule
{
    List<string> Check(RollingPolicyAckMilestone item);
}

public sealed record RollingPolicyAckMilestoneEmployeeIdLengthRule(int MaxLength) : IRollingPolicyAckMilestoneRule
{
    public List<string> Check(RollingPolicyAckMilestone item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPolicyAckMilestoneRequiredExternalRefRule(string Message) : IRollingPolicyAckMilestoneRule
{
    public List<string> Check(RollingPolicyAckMilestone item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPolicyAckMilestoneRequiredEmployeeIdRule(string Message) : IRollingPolicyAckMilestoneRule
{
    public List<string> Check(RollingPolicyAckMilestone item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPolicyAckMilestoneRequiredTitleRule(string Message) : IRollingPolicyAckMilestoneRule
{
    public List<string> Check(RollingPolicyAckMilestone item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling policy ack milestone.</summary>
public sealed record RollingPolicyAckMilestoneValidator(List<IRollingPolicyAckMilestoneRule> Rules)
{
    public List<string> Validate(RollingPolicyAckMilestone item)
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

    public void RequireValid(RollingPolicyAckMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPolicyAckMilestone> items)
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

/// <summary>Functions of rolling policy ack milestone rules.</summary>
public static class RollingPolicyAckMilestoneRules
{
    public static RollingPolicyAckMilestoneValidator DefaultRollingPolicyAckMilestoneValidator()
    {
        return new RollingPolicyAckMilestoneValidator(
            new List<IRollingPolicyAckMilestoneRule>
            {
                new RollingPolicyAckMilestoneEmployeeIdLengthRule(1000),
                new RollingPolicyAckMilestoneRequiredExternalRefRule("rolling policy ack milestone needs this field"),
                new RollingPolicyAckMilestoneRequiredEmployeeIdRule("rolling policy ack milestone needs a value"),
                new RollingPolicyAckMilestoneRequiredTitleRule("rolling policy ack milestone needs an entry"),
            });
    }
}
