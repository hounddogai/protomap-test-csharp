namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>A rule a policy ack audit must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckAuditRule
{
    List<string> Check(PolicyAckAudit item);
}

public sealed record PolicyAckAuditThresholdRangeRule(double Min, double Max) : IPolicyAckAuditRule
{
    public List<string> Check(PolicyAckAudit item)
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

public sealed record PolicyAckAuditSummaryLengthRule(int MaxLength) : IPolicyAckAuditRule
{
    public List<string> Check(PolicyAckAudit item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAuditDiscountRangeRule(double Min, double Max) : IPolicyAckAuditRule
{
    public List<string> Check(PolicyAckAudit item)
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

public sealed record PolicyAckAuditRequiredCodeRule(string Message) : IPolicyAckAuditRule
{
    public List<string> Check(PolicyAckAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAuditRequiredEmployeeIdRule(string Message) : IPolicyAckAuditRule
{
    public List<string> Check(PolicyAckAudit item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckAuditWindowDaysRangeRule(int Min, int Max) : IPolicyAckAuditRule
{
    public List<string> Check(PolicyAckAudit item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack audit.</summary>
public sealed record PolicyAckAuditValidator(List<IPolicyAckAuditRule> Rules)
{
    public List<string> Validate(PolicyAckAudit item)
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

    public void RequireValid(PolicyAckAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckAudit> items)
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

/// <summary>Functions of policy ack audit rules.</summary>
public static class PolicyAckAuditRules
{
    public static PolicyAckAuditValidator DefaultPolicyAckAuditValidator()
    {
        return new PolicyAckAuditValidator(
            new List<IPolicyAckAuditRule>
            {
                new PolicyAckAuditThresholdRangeRule(7.2, 4007.7),
                new PolicyAckAuditSummaryLengthRule(16),
                new PolicyAckAuditDiscountRangeRule(8.9, 2732.4),
                new PolicyAckAuditRequiredCodeRule("policy ack audit needs this field"),
                new PolicyAckAuditRequiredEmployeeIdRule("policy ack audit needs this field"),
                new PolicyAckAuditWindowDaysRangeRule(32, 1000),
            });
    }
}
