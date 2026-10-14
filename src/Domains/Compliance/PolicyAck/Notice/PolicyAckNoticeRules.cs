namespace Acme.Workforce.Compliance.PolicyAck.Notice;

/// <summary>A rule a policy ack notice must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckNoticeRule
{
    List<string> Check(PolicyAckNotice item);
}

public sealed record PolicyAckNoticeTeamLengthRule(int MaxLength) : IPolicyAckNoticeRule
{
    public List<string> Check(PolicyAckNotice item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckNoticeCurrencyLengthRule(int MaxLength) : IPolicyAckNoticeRule
{
    public List<string> Check(PolicyAckNotice item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckNoticeRequiredCodeRule(string Message) : IPolicyAckNoticeRule
{
    public List<string> Check(PolicyAckNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack notice.</summary>
public sealed record PolicyAckNoticeValidator(List<IPolicyAckNoticeRule> Rules)
{
    public List<string> Validate(PolicyAckNotice item)
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

    public void RequireValid(PolicyAckNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckNotice> items)
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

/// <summary>Functions of policy ack notice rules.</summary>
public static class PolicyAckNoticeRules
{
    public static PolicyAckNoticeValidator DefaultPolicyAckNoticeValidator()
    {
        return new PolicyAckNoticeValidator(
            new List<IPolicyAckNoticeRule>
            {
                new PolicyAckNoticeTeamLengthRule(120),
                new PolicyAckNoticeCurrencyLengthRule(255),
                new PolicyAckNoticeRequiredCodeRule("policy ack notice needs this field"),
            });
    }
}
