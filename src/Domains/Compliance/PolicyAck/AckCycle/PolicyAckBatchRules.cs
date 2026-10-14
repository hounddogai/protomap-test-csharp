namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack batch must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckBatchRule
{
    List<string> Check(PolicyAckBatch item);
}

public sealed record PolicyAckBatchCreatedAtRangeRule(int Min, int Max) : IPolicyAckBatchRule
{
    public List<string> Check(PolicyAckBatch item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBatchRetriesRangeRule(int Min, int Max) : IPolicyAckBatchRule
{
    public List<string> Check(PolicyAckBatch item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyAckBatchEmployeeIdLengthRule(int MaxLength) : IPolicyAckBatchRule
{
    public List<string> Check(PolicyAckBatch item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBatchRequiredContactEmailRule(string Message) : IPolicyAckBatchRule
{
    public List<string> Check(PolicyAckBatch item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBatchAllowedContactEmailRule(List<string> Allowed) : IPolicyAckBatchRule
{
    public List<string> Check(PolicyAckBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckBatchRatioRangeRule(double Min, double Max) : IPolicyAckBatchRule
{
    public List<string> Check(PolicyAckBatch item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a policy ack batch.</summary>
public sealed record PolicyAckBatchValidator(List<IPolicyAckBatchRule> Rules)
{
    public List<string> Validate(PolicyAckBatch item)
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

    public void RequireValid(PolicyAckBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckBatch> items)
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

/// <summary>Functions of policy ack batch rules.</summary>
public static class PolicyAckBatchRules
{
    public static PolicyAckBatchValidator DefaultPolicyAckBatchValidator()
    {
        return new PolicyAckBatchValidator(
            new List<IPolicyAckBatchRule>
            {
                new PolicyAckBatchCreatedAtRangeRule(120, 255),
                new PolicyAckBatchRetriesRangeRule(32, 86400),
                new PolicyAckBatchEmployeeIdLengthRule(80),
                new PolicyAckBatchRequiredContactEmailRule("policy ack batch needs a value"),
                new PolicyAckBatchAllowedContactEmailRule(new List<string> { "fallback" }),
                new PolicyAckBatchRatioRangeRule(9.4, 2883.6),
            });
    }
}
