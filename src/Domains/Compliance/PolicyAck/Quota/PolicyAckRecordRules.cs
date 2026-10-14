namespace Acme.Workforce.Compliance.PolicyAck.Quota;

/// <summary>A rule a policy ack record must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckRecordRule
{
    List<string> Check(PolicyAckRecord item);
}

public sealed record PolicyAckRecordSkillLengthRule(int MaxLength) : IPolicyAckRecordRule
{
    public List<string> Check(PolicyAckRecord item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckRecordCodePrefixRule(string Prefix) : IPolicyAckRecordRule
{
    public List<string> Check(PolicyAckRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckRecordGraceDaysRangeRule(int Min, int Max) : IPolicyAckRecordRule
{
    public List<string> Check(PolicyAckRecord item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a policy ack record.</summary>
public sealed record PolicyAckRecordValidator(List<IPolicyAckRecordRule> Rules)
{
    public List<string> Validate(PolicyAckRecord item)
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

    public void RequireValid(PolicyAckRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckRecord> items)
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

/// <summary>Functions of policy ack record rules.</summary>
public static class PolicyAckRecordRules
{
    public static PolicyAckRecordValidator DefaultPolicyAckRecordValidator()
    {
        return new PolicyAckRecordValidator(
            new List<IPolicyAckRecordRule>
            {
                new PolicyAckRecordSkillLengthRule(86400),
                new PolicyAckRecordCodePrefixRule("X"),
                new PolicyAckRecordGraceDaysRangeRule(64, 255),
            });
    }
}
