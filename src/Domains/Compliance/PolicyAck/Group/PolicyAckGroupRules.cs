namespace Acme.Workforce.Compliance.PolicyAck.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack group must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckGroupRule
{
    List<string> Check(PolicyAckGroup item);
}

public sealed record PolicyAckGroupScoreRangeRule(double Min, double Max) : IPolicyAckGroupRule
{
    public List<string> Check(PolicyAckGroup item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyAckGroupTitleLengthRule(int MaxLength) : IPolicyAckGroupRule
{
    public List<string> Check(PolicyAckGroup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckGroupSegmentLengthRule(int MaxLength) : IPolicyAckGroupRule
{
    public List<string> Check(PolicyAckGroup item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckGroupRequiredCodeRule(string Message) : IPolicyAckGroupRule
{
    public List<string> Check(PolicyAckGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckGroupTenureMonthsRangeRule(int Min, int Max) : IPolicyAckGroupRule
{
    public List<string> Check(PolicyAckGroup item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyAckGroupAllowedSegmentRule(List<string> Allowed) : IPolicyAckGroupRule
{
    public List<string> Check(PolicyAckGroup item)
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

/// <summary>Runs every rule of a policy ack group.</summary>
public sealed record PolicyAckGroupValidator(List<IPolicyAckGroupRule> Rules)
{
    public List<string> Validate(PolicyAckGroup item)
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

    public void RequireValid(PolicyAckGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckGroup> items)
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

/// <summary>Functions of policy ack group rules.</summary>
public static class PolicyAckGroupRules
{
    public static PolicyAckGroupValidator DefaultPolicyAckGroupValidator()
    {
        return new PolicyAckGroupValidator(
            new List<IPolicyAckGroupRule>
            {
                new PolicyAckGroupScoreRangeRule(4.6, 4995.2),
                new PolicyAckGroupTitleLengthRule(32),
                new PolicyAckGroupSegmentLengthRule(80),
                new PolicyAckGroupRequiredCodeRule("policy ack group needs a value"),
                new PolicyAckGroupTenureMonthsRangeRule(16, 86400),
                new PolicyAckGroupAllowedSegmentRule(new List<string> { "global" }),
            });
    }
}
