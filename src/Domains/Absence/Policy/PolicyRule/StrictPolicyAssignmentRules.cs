namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict policy assignment must pass. Check returns its problems, or none.</summary>
public interface IStrictPolicyAssignmentRule
{
    List<string> Check(StrictPolicyAssignment item);
}

public sealed record StrictPolicyAssignmentCapacityRangeRule(int Min, int Max) : IStrictPolicyAssignmentRule
{
    public List<string> Check(StrictPolicyAssignment item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAssignmentRequiredExternalRefRule(string Message) : IStrictPolicyAssignmentRule
{
    public List<string> Check(StrictPolicyAssignment item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAssignmentAllowedCodeRule(List<string> Allowed) : IStrictPolicyAssignmentRule
{
    public List<string> Check(StrictPolicyAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAssignmentRequiredLabelRule(string Message) : IStrictPolicyAssignmentRule
{
    public List<string> Check(StrictPolicyAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAssignmentSegmentLengthRule(int MaxLength) : IStrictPolicyAssignmentRule
{
    public List<string> Check(StrictPolicyAssignment item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAssignmentRequiredSegmentRule(string Message) : IStrictPolicyAssignmentRule
{
    public List<string> Check(StrictPolicyAssignment item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict policy assignment.</summary>
public sealed record StrictPolicyAssignmentValidator(List<IStrictPolicyAssignmentRule> Rules)
{
    public List<string> Validate(StrictPolicyAssignment item)
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

    public void RequireValid(StrictPolicyAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPolicyAssignment> items)
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

/// <summary>Functions of strict policy assignment rules.</summary>
public static class StrictPolicyAssignmentRules
{
    public static StrictPolicyAssignmentValidator DefaultStrictPolicyAssignmentValidator()
    {
        return new StrictPolicyAssignmentValidator(
            new List<IStrictPolicyAssignmentRule>
            {
                new StrictPolicyAssignmentCapacityRangeRule(16, 16),
                new StrictPolicyAssignmentRequiredExternalRefRule("strict policy assignment needs an entry"),
                new StrictPolicyAssignmentAllowedCodeRule(new List<string> { "regional", "strict", "primary" }),
                new StrictPolicyAssignmentRequiredLabelRule("strict policy assignment needs this field"),
                new StrictPolicyAssignmentSegmentLengthRule(3600),
                new StrictPolicyAssignmentRequiredSegmentRule("strict policy assignment needs this field"),
            });
    }
}
