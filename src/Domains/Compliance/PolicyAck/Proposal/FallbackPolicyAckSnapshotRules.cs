namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

/// <summary>A rule a fallback policy ack snapshot must pass. Check returns its problems, or none.</summary>
public interface IFallbackPolicyAckSnapshotRule
{
    List<string> Check(FallbackPolicyAckSnapshot item);
}

public sealed record FallbackPolicyAckSnapshotChecksumLengthRule(int MaxLength) : IFallbackPolicyAckSnapshotRule
{
    public List<string> Check(FallbackPolicyAckSnapshot item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckSnapshotRequiredChecksumRule(string Message) : IFallbackPolicyAckSnapshotRule
{
    public List<string> Check(FallbackPolicyAckSnapshot item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckSnapshotPriorityRangeRule(int Min, int Max) : IFallbackPolicyAckSnapshotRule
{
    public List<string> Check(FallbackPolicyAckSnapshot item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback policy ack snapshot.</summary>
public sealed record FallbackPolicyAckSnapshotValidator(List<IFallbackPolicyAckSnapshotRule> Rules)
{
    public List<string> Validate(FallbackPolicyAckSnapshot item)
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

    public void RequireValid(FallbackPolicyAckSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPolicyAckSnapshot> items)
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

/// <summary>Functions of fallback policy ack snapshot rules.</summary>
public static class FallbackPolicyAckSnapshotRules
{
    public static FallbackPolicyAckSnapshotValidator DefaultFallbackPolicyAckSnapshotValidator()
    {
        return new FallbackPolicyAckSnapshotValidator(
            new List<IFallbackPolicyAckSnapshotRule>
            {
                new FallbackPolicyAckSnapshotChecksumLengthRule(3600),
                new FallbackPolicyAckSnapshotRequiredChecksumRule("fallback policy ack snapshot needs an entry"),
                new FallbackPolicyAckSnapshotPriorityRangeRule(16, 16),
            });
    }
}
