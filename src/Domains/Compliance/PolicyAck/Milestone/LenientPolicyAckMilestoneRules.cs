namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

/// <summary>A rule a lenient policy ack milestone must pass. Check returns its problems, or none.</summary>
public interface ILenientPolicyAckMilestoneRule
{
    List<string> Check(LenientPolicyAckMilestone item);
}

public sealed record LenientPolicyAckMilestoneRequiredChecksumRule(string Message) : ILenientPolicyAckMilestoneRule
{
    public List<string> Check(LenientPolicyAckMilestone item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckMilestonePinnedGuardRule(double Limit) : ILenientPolicyAckMilestoneRule
{
    public List<string> Check(LenientPolicyAckMilestone item)
    {
        if (item.Pinned && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckMilestoneChecksumLengthRule(int MaxLength) : ILenientPolicyAckMilestoneRule
{
    public List<string> Check(LenientPolicyAckMilestone item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckMilestoneFteRangeRule(double Min, double Max) : ILenientPolicyAckMilestoneRule
{
    public List<string> Check(LenientPolicyAckMilestone item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckMilestoneLabelLengthRule(int MaxLength) : ILenientPolicyAckMilestoneRule
{
    public List<string> Check(LenientPolicyAckMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckMilestoneCodeLengthRule(int MaxLength) : ILenientPolicyAckMilestoneRule
{
    public List<string> Check(LenientPolicyAckMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient policy ack milestone.</summary>
public sealed record LenientPolicyAckMilestoneValidator(List<ILenientPolicyAckMilestoneRule> Rules)
{
    public List<string> Validate(LenientPolicyAckMilestone item)
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

    public void RequireValid(LenientPolicyAckMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPolicyAckMilestone> items)
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

/// <summary>Functions of lenient policy ack milestone rules.</summary>
public static class LenientPolicyAckMilestoneRules
{
    public static LenientPolicyAckMilestoneValidator DefaultLenientPolicyAckMilestoneValidator()
    {
        return new LenientPolicyAckMilestoneValidator(
            new List<ILenientPolicyAckMilestoneRule>
            {
                new LenientPolicyAckMilestoneRequiredChecksumRule("lenient policy ack milestone needs this field"),
                new LenientPolicyAckMilestonePinnedGuardRule(1.3),
                new LenientPolicyAckMilestoneChecksumLengthRule(3600),
                new LenientPolicyAckMilestoneFteRangeRule(8.8, 163.0),
                new LenientPolicyAckMilestoneLabelLengthRule(3600),
                new LenientPolicyAckMilestoneCodeLengthRule(3600),
            });
    }
}
