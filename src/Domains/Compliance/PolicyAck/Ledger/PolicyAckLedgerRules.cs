namespace Acme.Workforce.Compliance.PolicyAck.Ledger;

/// <summary>A rule a policy ack ledger must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckLedgerRule
{
    List<string> Check(PolicyAckLedger item);
}

public sealed record PolicyAckLedgerCodeLengthRule(int MaxLength) : IPolicyAckLedgerRule
{
    public List<string> Check(PolicyAckLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckLedgerRequiredNameRule(string Message) : IPolicyAckLedgerRule
{
    public List<string> Check(PolicyAckLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckLedgerRequiredReviewerRule(string Message) : IPolicyAckLedgerRule
{
    public List<string> Check(PolicyAckLedger item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack ledger.</summary>
public sealed record PolicyAckLedgerValidator(List<IPolicyAckLedgerRule> Rules)
{
    public List<string> Validate(PolicyAckLedger item)
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

    public void RequireValid(PolicyAckLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckLedger> items)
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

/// <summary>Functions of policy ack ledger rules.</summary>
public static class PolicyAckLedgerRules
{
    public static PolicyAckLedgerValidator DefaultPolicyAckLedgerValidator()
    {
        return new PolicyAckLedgerValidator(
            new List<IPolicyAckLedgerRule>
            {
                new PolicyAckLedgerCodeLengthRule(120),
                new PolicyAckLedgerRequiredNameRule("policy ack ledger needs a value"),
                new PolicyAckLedgerRequiredReviewerRule("policy ack ledger needs an entry"),
            });
    }
}
