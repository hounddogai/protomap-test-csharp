namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ack score must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckScoreRule
{
    List<string> Check(PolicyAckScore item);
}

public sealed record PolicyAckScoreChecksumLengthRule(int MaxLength) : IPolicyAckScoreRule
{
    public List<string> Check(PolicyAckScore item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckScoreRateRangeRule(double Min, double Max) : IPolicyAckScoreRule
{
    public List<string> Check(PolicyAckScore item)
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

public sealed record PolicyAckScoreCodeLengthRule(int MaxLength) : IPolicyAckScoreRule
{
    public List<string> Check(PolicyAckScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckScoreAllowedChecksumRule(List<string> Allowed) : IPolicyAckScoreRule
{
    public List<string> Check(PolicyAckScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckScoreRequiredCountryRule(string Message) : IPolicyAckScoreRule
{
    public List<string> Check(PolicyAckScore item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckScoreRequiredChecksumRule(string Message) : IPolicyAckScoreRule
{
    public List<string> Check(PolicyAckScore item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack score.</summary>
public sealed record PolicyAckScoreValidator(List<IPolicyAckScoreRule> Rules)
{
    public List<string> Validate(PolicyAckScore item)
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

    public void RequireValid(PolicyAckScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckScore> items)
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

/// <summary>Functions of policy ack score rules.</summary>
public static class PolicyAckScoreRules
{
    public static PolicyAckScoreValidator DefaultPolicyAckScoreValidator()
    {
        return new PolicyAckScoreValidator(
            new List<IPolicyAckScoreRule>
            {
                new PolicyAckScoreChecksumLengthRule(32),
                new PolicyAckScoreRateRangeRule(0.3, 1840.3),
                new PolicyAckScoreCodeLengthRule(32),
                new PolicyAckScoreAllowedChecksumRule(new List<string> { "tiered", "seasonal" }),
                new PolicyAckScoreRequiredCountryRule("policy ack score needs an entry"),
                new PolicyAckScoreRequiredChecksumRule("policy ack score needs a value"),
            });
    }
}
