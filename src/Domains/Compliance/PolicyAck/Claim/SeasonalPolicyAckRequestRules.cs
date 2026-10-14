namespace Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>A rule a seasonal policy ack request must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPolicyAckRequestRule
{
    List<string> Check(SeasonalPolicyAckRequest item);
}

public sealed record SeasonalPolicyAckRequestCodeLengthRule(int MaxLength) : ISeasonalPolicyAckRequestRule
{
    public List<string> Check(SeasonalPolicyAckRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPolicyAckRequestCodePrefixRule(string Prefix) : ISeasonalPolicyAckRequestRule
{
    public List<string> Check(SeasonalPolicyAckRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPolicyAckRequestApprovedGuardRule(double Limit) : ISeasonalPolicyAckRequestRule
{
    public List<string> Check(SeasonalPolicyAckRequest item)
    {
        if (item.Approved && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPolicyAckRequestRequiredCodeRule(string Message) : ISeasonalPolicyAckRequestRule
{
    public List<string> Check(SeasonalPolicyAckRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPolicyAckRequestLabelLengthRule(int MaxLength) : ISeasonalPolicyAckRequestRule
{
    public List<string> Check(SeasonalPolicyAckRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal policy ack request.</summary>
public sealed record SeasonalPolicyAckRequestValidator(List<ISeasonalPolicyAckRequestRule> Rules)
{
    public List<string> Validate(SeasonalPolicyAckRequest item)
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

    public void RequireValid(SeasonalPolicyAckRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPolicyAckRequest> items)
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

/// <summary>Functions of seasonal policy ack request rules.</summary>
public static class SeasonalPolicyAckRequestRules
{
    public static SeasonalPolicyAckRequestValidator DefaultSeasonalPolicyAckRequestValidator()
    {
        return new SeasonalPolicyAckRequestValidator(
            new List<ISeasonalPolicyAckRequestRule>
            {
                new SeasonalPolicyAckRequestCodeLengthRule(1000),
                new SeasonalPolicyAckRequestCodePrefixRule("OPS"),
                new SeasonalPolicyAckRequestApprovedGuardRule(3.1),
                new SeasonalPolicyAckRequestRequiredCodeRule("seasonal policy ack request needs a value"),
                new SeasonalPolicyAckRequestLabelLengthRule(86400),
            });
    }
}
