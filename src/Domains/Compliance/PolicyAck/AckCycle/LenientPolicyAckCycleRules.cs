namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient policy ack cycle must pass. Check returns its problems, or none.</summary>
public interface ILenientPolicyAckCycleRule
{
    List<string> Check(LenientPolicyAckCycle item);
}

public sealed record LenientPolicyAckCycleAllowedContactEmailRule(List<string> Allowed) : ILenientPolicyAckCycleRule
{
    public List<string> Check(LenientPolicyAckCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckCycleRequiredCodeRule(string Message) : ILenientPolicyAckCycleRule
{
    public List<string> Check(LenientPolicyAckCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckCycleJobCodeLengthRule(int MaxLength) : ILenientPolicyAckCycleRule
{
    public List<string> Check(LenientPolicyAckCycle item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckCycleRequiredGradeRule(string Message) : ILenientPolicyAckCycleRule
{
    public List<string> Check(LenientPolicyAckCycle item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyAckCycleRequiredJobCodeRule(string Message) : ILenientPolicyAckCycleRule
{
    public List<string> Check(LenientPolicyAckCycle item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient policy ack cycle.</summary>
public sealed record LenientPolicyAckCycleValidator(List<ILenientPolicyAckCycleRule> Rules)
{
    public List<string> Validate(LenientPolicyAckCycle item)
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

    public void RequireValid(LenientPolicyAckCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPolicyAckCycle> items)
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

/// <summary>Functions of lenient policy ack cycle rules.</summary>
public static class LenientPolicyAckCycleRules
{
    public static LenientPolicyAckCycleValidator DefaultLenientPolicyAckCycleValidator()
    {
        return new LenientPolicyAckCycleValidator(
            new List<ILenientPolicyAckCycleRule>
            {
                new LenientPolicyAckCycleAllowedContactEmailRule(new List<string>()),
                new LenientPolicyAckCycleRequiredCodeRule("lenient policy ack cycle needs this field"),
                new LenientPolicyAckCycleJobCodeLengthRule(16),
                new LenientPolicyAckCycleRequiredGradeRule("lenient policy ack cycle needs this field"),
                new LenientPolicyAckCycleRequiredJobCodeRule("lenient policy ack cycle needs this field"),
            });
    }
}
