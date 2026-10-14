namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>A rule a policy ack variance must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckVarianceRule
{
    List<string> Check(PolicyAckVariance item);
}

public sealed record PolicyAckVarianceRequiredCodeRule(string Message) : IPolicyAckVarianceRule
{
    public List<string> Check(PolicyAckVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckVarianceAttemptsRangeRule(int Min, int Max) : IPolicyAckVarianceRule
{
    public List<string> Check(PolicyAckVariance item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyAckVarianceArchivedGuardRule(double Limit) : IPolicyAckVarianceRule
{
    public List<string> Check(PolicyAckVariance item)
    {
        if (item.Archived && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckVarianceRequiredEmployeeIdRule(string Message) : IPolicyAckVarianceRule
{
    public List<string> Check(PolicyAckVariance item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckVarianceRequiredCurrencyRule(string Message) : IPolicyAckVarianceRule
{
    public List<string> Check(PolicyAckVariance item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack variance.</summary>
public sealed record PolicyAckVarianceValidator(List<IPolicyAckVarianceRule> Rules)
{
    public List<string> Validate(PolicyAckVariance item)
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

    public void RequireValid(PolicyAckVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckVariance> items)
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

/// <summary>Functions of policy ack variance rules.</summary>
public static class PolicyAckVarianceRules
{
    public static PolicyAckVarianceValidator DefaultPolicyAckVarianceValidator()
    {
        return new PolicyAckVarianceValidator(
            new List<IPolicyAckVarianceRule>
            {
                new PolicyAckVarianceRequiredCodeRule("policy ack variance needs a value"),
                new PolicyAckVarianceAttemptsRangeRule(255, 255),
                new PolicyAckVarianceArchivedGuardRule(6.4),
                new PolicyAckVarianceRequiredEmployeeIdRule("policy ack variance needs this field"),
                new PolicyAckVarianceRequiredCurrencyRule("policy ack variance needs an entry"),
            });
    }
}
