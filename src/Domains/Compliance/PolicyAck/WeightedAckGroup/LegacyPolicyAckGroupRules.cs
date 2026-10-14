namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>A rule a legacy policy ack group must pass. Check returns its problems, or none.</summary>
public interface ILegacyPolicyAckGroupRule
{
    List<string> Check(LegacyPolicyAckGroup item);
}

public sealed record LegacyPolicyAckGroupLimitRangeRule(int Min, int Max) : ILegacyPolicyAckGroupRule
{
    public List<string> Check(LegacyPolicyAckGroup item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyPolicyAckGroupPriorityRangeRule(int Min, int Max) : ILegacyPolicyAckGroupRule
{
    public List<string> Check(LegacyPolicyAckGroup item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPolicyAckGroupRequiredCodeRule(string Message) : ILegacyPolicyAckGroupRule
{
    public List<string> Check(LegacyPolicyAckGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPolicyAckGroupLabelLengthRule(int MaxLength) : ILegacyPolicyAckGroupRule
{
    public List<string> Check(LegacyPolicyAckGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy policy ack group.</summary>
public sealed record LegacyPolicyAckGroupValidator(List<ILegacyPolicyAckGroupRule> Rules)
{
    public List<string> Validate(LegacyPolicyAckGroup item)
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

    public void RequireValid(LegacyPolicyAckGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPolicyAckGroup> items)
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

/// <summary>Functions of legacy policy ack group rules.</summary>
public static class LegacyPolicyAckGroupRules
{
    public static LegacyPolicyAckGroupValidator DefaultLegacyPolicyAckGroupValidator()
    {
        return new LegacyPolicyAckGroupValidator(
            new List<ILegacyPolicyAckGroupRule>
            {
                new LegacyPolicyAckGroupLimitRangeRule(16, 1000),
                new LegacyPolicyAckGroupPriorityRangeRule(16, 1000),
                new LegacyPolicyAckGroupRequiredCodeRule("legacy policy ack group needs a value"),
                new LegacyPolicyAckGroupLabelLengthRule(120),
            });
    }
}
