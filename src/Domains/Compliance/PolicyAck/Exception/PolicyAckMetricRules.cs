namespace Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>A rule a policy ack metric must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckMetricRule
{
    List<string> Check(PolicyAckMetric item);
}

public sealed record PolicyAckMetricRequiredExternalRefRule(string Message) : IPolicyAckMetricRule
{
    public List<string> Check(PolicyAckMetric item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckMetricVisibleGuardRule(int Limit) : IPolicyAckMetricRule
{
    public List<string> Check(PolicyAckMetric item)
    {
        if (item.Visible && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckMetricCodeLengthRule(int MaxLength) : IPolicyAckMetricRule
{
    public List<string> Check(PolicyAckMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckMetricCapacityRangeRule(int Min, int Max) : IPolicyAckMetricRule
{
    public List<string> Check(PolicyAckMetric item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a policy ack metric.</summary>
public sealed record PolicyAckMetricValidator(List<IPolicyAckMetricRule> Rules)
{
    public List<string> Validate(PolicyAckMetric item)
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

    public void RequireValid(PolicyAckMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckMetric> items)
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

/// <summary>Functions of policy ack metric rules.</summary>
public static class PolicyAckMetricRules
{
    public static PolicyAckMetricValidator DefaultPolicyAckMetricValidator()
    {
        return new PolicyAckMetricValidator(
            new List<IPolicyAckMetricRule>
            {
                new PolicyAckMetricRequiredExternalRefRule("policy ack metric needs a value"),
                new PolicyAckMetricVisibleGuardRule(16),
                new PolicyAckMetricCodeLengthRule(3600),
                new PolicyAckMetricCapacityRangeRule(64, 255),
            });
    }
}
