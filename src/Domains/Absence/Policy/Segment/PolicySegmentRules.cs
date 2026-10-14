namespace Acme.Workforce.Absence.Policy.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy segment must pass. Check returns its problems, or none.</summary>
public interface IPolicySegmentRule
{
    List<string> Check(PolicySegment item);
}

public sealed record PolicySegmentRequiredNameRule(string Message) : IPolicySegmentRule
{
    public List<string> Check(PolicySegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicySegmentAllowedSegmentRule(List<string> Allowed) : IPolicySegmentRule
{
    public List<string> Check(PolicySegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PolicySegmentNameLengthRule(int MaxLength) : IPolicySegmentRule
{
    public List<string> Check(PolicySegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicySegmentRequiredSegmentRule(string Message) : IPolicySegmentRule
{
    public List<string> Check(PolicySegment item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy segment.</summary>
public sealed record PolicySegmentValidator(List<IPolicySegmentRule> Rules)
{
    public List<string> Validate(PolicySegment item)
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

    public void RequireValid(PolicySegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicySegment> items)
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

/// <summary>Functions of policy segment rules.</summary>
public static class PolicySegmentRules
{
    public static PolicySegmentValidator DefaultPolicySegmentValidator()
    {
        return new PolicySegmentValidator(
            new List<IPolicySegmentRule>
            {
                new PolicySegmentRequiredNameRule("policy segment needs this field"),
                new PolicySegmentAllowedSegmentRule(new List<string>()),
                new PolicySegmentNameLengthRule(32),
                new PolicySegmentRequiredSegmentRule("policy segment needs this field"),
            });
    }
}
