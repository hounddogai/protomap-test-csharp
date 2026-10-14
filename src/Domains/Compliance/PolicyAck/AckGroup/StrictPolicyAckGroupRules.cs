namespace Acme.Workforce.Compliance.PolicyAck.AckGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict policy ack group must pass. Check returns its problems, or none.</summary>
public interface IStrictPolicyAckGroupRule
{
    List<string> Check(StrictPolicyAckGroup item);
}

public sealed record StrictPolicyAckGroupAllowedCodeRule(List<string> Allowed) : IStrictPolicyAckGroupRule
{
    public List<string> Check(StrictPolicyAckGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAckGroupUnitLengthRule(int MaxLength) : IStrictPolicyAckGroupRule
{
    public List<string> Check(StrictPolicyAckGroup item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAckGroupRequiredNameRule(string Message) : IStrictPolicyAckGroupRule
{
    public List<string> Check(StrictPolicyAckGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAckGroupStartsAtRangeRule(int Min, int Max) : IStrictPolicyAckGroupRule
{
    public List<string> Check(StrictPolicyAckGroup item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAckGroupSequenceRangeRule(int Min, int Max) : IStrictPolicyAckGroupRule
{
    public List<string> Check(StrictPolicyAckGroup item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyAckGroupNameLengthRule(int MaxLength) : IStrictPolicyAckGroupRule
{
    public List<string> Check(StrictPolicyAckGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict policy ack group.</summary>
public sealed record StrictPolicyAckGroupValidator(List<IStrictPolicyAckGroupRule> Rules)
{
    public List<string> Validate(StrictPolicyAckGroup item)
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

    public void RequireValid(StrictPolicyAckGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPolicyAckGroup> items)
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

/// <summary>Functions of strict policy ack group rules.</summary>
public static class StrictPolicyAckGroupRules
{
    public static StrictPolicyAckGroupValidator DefaultStrictPolicyAckGroupValidator()
    {
        return new StrictPolicyAckGroupValidator(
            new List<IStrictPolicyAckGroupRule>
            {
                new StrictPolicyAckGroupAllowedCodeRule(new List<string> { "legacy", "strict", "fallback" }),
                new StrictPolicyAckGroupUnitLengthRule(86400),
                new StrictPolicyAckGroupRequiredNameRule("strict policy ack group needs this field"),
                new StrictPolicyAckGroupStartsAtRangeRule(64, 255),
                new StrictPolicyAckGroupSequenceRangeRule(255, 3600),
                new StrictPolicyAckGroupNameLengthRule(32),
            });
    }
}
