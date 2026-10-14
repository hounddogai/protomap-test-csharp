namespace Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>A rule a pending policy ack group must pass. Check returns its problems, or none.</summary>
public interface IPendingPolicyAckGroupRule
{
    List<string> Check(PendingPolicyAckGroup item);
}

public sealed record PendingPolicyAckGroupBudgetRangeRule(double Min, double Max) : IPendingPolicyAckGroupRule
{
    public List<string> Check(PendingPolicyAckGroup item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingPolicyAckGroupThresholdRangeRule(double Min, double Max) : IPendingPolicyAckGroupRule
{
    public List<string> Check(PendingPolicyAckGroup item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingPolicyAckGroupTitleLengthRule(int MaxLength) : IPendingPolicyAckGroupRule
{
    public List<string> Check(PendingPolicyAckGroup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPolicyAckGroupAmountRangeRule(double Min, double Max) : IPendingPolicyAckGroupRule
{
    public List<string> Check(PendingPolicyAckGroup item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending policy ack group.</summary>
public sealed record PendingPolicyAckGroupValidator(List<IPendingPolicyAckGroupRule> Rules)
{
    public List<string> Validate(PendingPolicyAckGroup item)
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

    public void RequireValid(PendingPolicyAckGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPolicyAckGroup> items)
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

/// <summary>Functions of pending policy ack group rules.</summary>
public static class PendingPolicyAckGroupRules
{
    public static PendingPolicyAckGroupValidator DefaultPendingPolicyAckGroupValidator()
    {
        return new PendingPolicyAckGroupValidator(
            new List<IPendingPolicyAckGroupRule>
            {
                new PendingPolicyAckGroupBudgetRangeRule(4.6, 4807.5),
                new PendingPolicyAckGroupThresholdRangeRule(3.6, 1508.0),
                new PendingPolicyAckGroupTitleLengthRule(86400),
                new PendingPolicyAckGroupAmountRangeRule(1.6, 150.4),
            });
    }
}
