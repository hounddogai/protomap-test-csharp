namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy assignment must pass. Check returns its problems, or none.</summary>
public interface IPolicyAssignmentRule
{
    List<string> Check(PolicyAssignment item);
}

public sealed record PolicyAssignmentTitleLengthRule(int MaxLength) : IPolicyAssignmentRule
{
    public List<string> Check(PolicyAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAssignmentRequiredBadgeRule(string Message) : IPolicyAssignmentRule
{
    public List<string> Check(PolicyAssignment item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAssignmentAllowedSegmentRule(List<string> Allowed) : IPolicyAssignmentRule
{
    public List<string> Check(PolicyAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy assignment.</summary>
public sealed record PolicyAssignmentValidator(List<IPolicyAssignmentRule> Rules)
{
    public List<string> Validate(PolicyAssignment item)
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

    public void RequireValid(PolicyAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAssignment> items)
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

/// <summary>Functions of policy assignment rules.</summary>
public static class PolicyAssignmentRules
{
    public static PolicyAssignmentValidator DefaultPolicyAssignmentValidator()
    {
        return new PolicyAssignmentValidator(
            new List<IPolicyAssignmentRule>
            {
                new PolicyAssignmentTitleLengthRule(16),
                new PolicyAssignmentRequiredBadgeRule("policy assignment needs this field"),
                new PolicyAssignmentAllowedSegmentRule(new List<string>()),
            });
    }
}
