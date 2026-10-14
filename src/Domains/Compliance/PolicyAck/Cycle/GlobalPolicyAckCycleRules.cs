namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

/// <summary>A rule a global policy ack cycle must pass. Check returns its problems, or none.</summary>
public interface IGlobalPolicyAckCycleRule
{
    List<string> Check(GlobalPolicyAckCycle item);
}

public sealed record GlobalPolicyAckCycleRequiredBadgeRule(string Message) : IGlobalPolicyAckCycleRule
{
    public List<string> Check(GlobalPolicyAckCycle item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyAckCycleRequiredTitleRule(string Message) : IGlobalPolicyAckCycleRule
{
    public List<string> Check(GlobalPolicyAckCycle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyAckCycleAttemptsRangeRule(int Min, int Max) : IGlobalPolicyAckCycleRule
{
    public List<string> Check(GlobalPolicyAckCycle item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalPolicyAckCycleTitleLengthRule(int MaxLength) : IGlobalPolicyAckCycleRule
{
    public List<string> Check(GlobalPolicyAckCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPolicyAckCycleRequiredPhoneRule(string Message) : IGlobalPolicyAckCycleRule
{
    public List<string> Check(GlobalPolicyAckCycle item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global policy ack cycle.</summary>
public sealed record GlobalPolicyAckCycleValidator(List<IGlobalPolicyAckCycleRule> Rules)
{
    public List<string> Validate(GlobalPolicyAckCycle item)
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

    public void RequireValid(GlobalPolicyAckCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPolicyAckCycle> items)
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

/// <summary>Functions of global policy ack cycle rules.</summary>
public static class GlobalPolicyAckCycleRules
{
    public static GlobalPolicyAckCycleValidator DefaultGlobalPolicyAckCycleValidator()
    {
        return new GlobalPolicyAckCycleValidator(
            new List<IGlobalPolicyAckCycleRule>
            {
                new GlobalPolicyAckCycleRequiredBadgeRule("global policy ack cycle needs an entry"),
                new GlobalPolicyAckCycleRequiredTitleRule("global policy ack cycle needs a value"),
                new GlobalPolicyAckCycleAttemptsRangeRule(16, 64),
                new GlobalPolicyAckCycleTitleLengthRule(255),
                new GlobalPolicyAckCycleRequiredPhoneRule("global policy ack cycle needs this field"),
            });
    }
}
