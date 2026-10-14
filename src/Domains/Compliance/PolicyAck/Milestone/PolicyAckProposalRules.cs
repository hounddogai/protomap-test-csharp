namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack proposal must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckProposalRule
{
    List<string> Check(PolicyAckProposal item);
}

public sealed record PolicyAckProposalRequiredSalaryBandRule(string Message) : IPolicyAckProposalRule
{
    public List<string> Check(PolicyAckProposal item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckProposalAllowedTitleRule(List<string> Allowed) : IPolicyAckProposalRule
{
    public List<string> Check(PolicyAckProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckProposalRequiredCostCenterRule(string Message) : IPolicyAckProposalRule
{
    public List<string> Check(PolicyAckProposal item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckProposalDiscountRangeRule(double Min, double Max) : IPolicyAckProposalRule
{
    public List<string> Check(PolicyAckProposal item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyAckProposalCodePrefixRule(string Prefix) : IPolicyAckProposalRule
{
    public List<string> Check(PolicyAckProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack proposal.</summary>
public sealed record PolicyAckProposalValidator(List<IPolicyAckProposalRule> Rules)
{
    public List<string> Validate(PolicyAckProposal item)
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

    public void RequireValid(PolicyAckProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckProposal> items)
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

/// <summary>Functions of policy ack proposal rules.</summary>
public static class PolicyAckProposalRules
{
    public static PolicyAckProposalValidator DefaultPolicyAckProposalValidator()
    {
        return new PolicyAckProposalValidator(
            new List<IPolicyAckProposalRule>
            {
                new PolicyAckProposalRequiredSalaryBandRule("policy ack proposal needs this field"),
                new PolicyAckProposalAllowedTitleRule(new List<string>()),
                new PolicyAckProposalRequiredCostCenterRule("policy ack proposal needs an entry"),
                new PolicyAckProposalDiscountRangeRule(1.9, 4652.4),
                new PolicyAckProposalCodePrefixRule("X"),
            });
    }
}
