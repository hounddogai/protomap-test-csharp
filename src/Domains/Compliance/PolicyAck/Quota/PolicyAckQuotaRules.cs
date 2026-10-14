namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack quota must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckQuotaRule
{
    List<string> Check(PolicyAckQuota item);
}

public sealed record PolicyAckQuotaAllowedCategoryRule(List<string> Allowed) : IPolicyAckQuotaRule
{
    public List<string> Check(PolicyAckQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is too long" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckQuotaRequiredCategoryRule(string Message) : IPolicyAckQuotaRule
{
    public List<string> Check(PolicyAckQuota item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckQuotaRateRangeRule(double Min, double Max) : IPolicyAckQuotaRule
{
    public List<string> Check(PolicyAckQuota item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyAckQuotaRequiredSummaryRule(string Message) : IPolicyAckQuotaRule
{
    public List<string> Check(PolicyAckQuota item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckQuotaRequiredTitleRule(string Message) : IPolicyAckQuotaRule
{
    public List<string> Check(PolicyAckQuota item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack quota.</summary>
public sealed record PolicyAckQuotaValidator(List<IPolicyAckQuotaRule> Rules)
{
    public List<string> Validate(PolicyAckQuota item)
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

    public void RequireValid(PolicyAckQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckQuota> items)
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

/// <summary>Functions of policy ack quota rules.</summary>
public static class PolicyAckQuotaRules
{
    public static PolicyAckQuotaValidator DefaultPolicyAckQuotaValidator()
    {
        return new PolicyAckQuotaValidator(
            new List<IPolicyAckQuotaRule>
            {
                new PolicyAckQuotaAllowedCategoryRule(new List<string> { "lenient", "default" }),
                new PolicyAckQuotaRequiredCategoryRule("policy ack quota needs an entry"),
                new PolicyAckQuotaRateRangeRule(7.0, 1585.0),
                new PolicyAckQuotaRequiredSummaryRule("policy ack quota needs this field"),
                new PolicyAckQuotaRequiredTitleRule("policy ack quota needs a value"),
            });
    }
}
