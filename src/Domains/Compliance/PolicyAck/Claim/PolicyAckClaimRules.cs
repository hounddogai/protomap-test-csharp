namespace Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>A rule a policy ack claim must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckClaimRule
{
    List<string> Check(PolicyAckClaim item);
}

public sealed record PolicyAckClaimCategoryLengthRule(int MaxLength) : IPolicyAckClaimRule
{
    public List<string> Check(PolicyAckClaim item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckClaimRequiredTitleRule(string Message) : IPolicyAckClaimRule
{
    public List<string> Check(PolicyAckClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckClaimTitleLengthRule(int MaxLength) : IPolicyAckClaimRule
{
    public List<string> Check(PolicyAckClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckClaimRequiredCategoryRule(string Message) : IPolicyAckClaimRule
{
    public List<string> Check(PolicyAckClaim item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckClaimAmountRangeRule(double Min, double Max) : IPolicyAckClaimRule
{
    public List<string> Check(PolicyAckClaim item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a policy ack claim.</summary>
public sealed record PolicyAckClaimValidator(List<IPolicyAckClaimRule> Rules)
{
    public List<string> Validate(PolicyAckClaim item)
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

    public void RequireValid(PolicyAckClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckClaim> items)
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

/// <summary>Functions of policy ack claim rules.</summary>
public static class PolicyAckClaimRules
{
    public static PolicyAckClaimValidator DefaultPolicyAckClaimValidator()
    {
        return new PolicyAckClaimValidator(
            new List<IPolicyAckClaimRule>
            {
                new PolicyAckClaimCategoryLengthRule(32),
                new PolicyAckClaimRequiredTitleRule("policy ack claim needs a value"),
                new PolicyAckClaimTitleLengthRule(32),
                new PolicyAckClaimRequiredCategoryRule("policy ack claim needs an entry"),
                new PolicyAckClaimAmountRangeRule(9.0, 496.1),
            });
    }
}
