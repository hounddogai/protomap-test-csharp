namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>A rule a scheduled policy ack quota must pass. Check returns its problems, or none.</summary>
public interface IScheduledPolicyAckQuotaRule
{
    List<string> Check(ScheduledPolicyAckQuota item);
}

public sealed record ScheduledPolicyAckQuotaGraceDaysRangeRule(int Min, int Max) : IScheduledPolicyAckQuotaRule
{
    public List<string> Check(ScheduledPolicyAckQuota item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledPolicyAckQuotaCodePrefixRule(string Prefix) : IScheduledPolicyAckQuotaRule
{
    public List<string> Check(ScheduledPolicyAckQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPolicyAckQuotaRequiredNameRule(string Message) : IScheduledPolicyAckQuotaRule
{
    public List<string> Check(ScheduledPolicyAckQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled policy ack quota.</summary>
public sealed record ScheduledPolicyAckQuotaValidator(List<IScheduledPolicyAckQuotaRule> Rules)
{
    public List<string> Validate(ScheduledPolicyAckQuota item)
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

    public void RequireValid(ScheduledPolicyAckQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPolicyAckQuota> items)
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

/// <summary>Functions of scheduled policy ack quota rules.</summary>
public static class ScheduledPolicyAckQuotaRules
{
    public static ScheduledPolicyAckQuotaValidator DefaultScheduledPolicyAckQuotaValidator()
    {
        return new ScheduledPolicyAckQuotaValidator(
            new List<IScheduledPolicyAckQuotaRule>
            {
                new ScheduledPolicyAckQuotaGraceDaysRangeRule(64, 80),
                new ScheduledPolicyAckQuotaCodePrefixRule("X"),
                new ScheduledPolicyAckQuotaRequiredNameRule("scheduled policy ack quota needs an entry"),
            });
    }
}
