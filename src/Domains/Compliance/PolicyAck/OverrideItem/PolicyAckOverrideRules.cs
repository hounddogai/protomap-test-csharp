namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>A rule a policy ack override must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckOverrideRule
{
    List<string> Check(PolicyAckOverride item);
}

public sealed record PolicyAckOverrideCodeLengthRule(int MaxLength) : IPolicyAckOverrideRule
{
    public List<string> Check(PolicyAckOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckOverrideRequiredCodeRule(string Message) : IPolicyAckOverrideRule
{
    public List<string> Check(PolicyAckOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckOverrideRequiredNotesRule(string Message) : IPolicyAckOverrideRule
{
    public List<string> Check(PolicyAckOverride item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckOverrideRequiredLabelRule(string Message) : IPolicyAckOverrideRule
{
    public List<string> Check(PolicyAckOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckOverrideCodePrefixRule(string Prefix) : IPolicyAckOverrideRule
{
    public List<string> Check(PolicyAckOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckOverrideReviewerLengthRule(int MaxLength) : IPolicyAckOverrideRule
{
    public List<string> Check(PolicyAckOverride item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack override.</summary>
public sealed record PolicyAckOverrideValidator(List<IPolicyAckOverrideRule> Rules)
{
    public List<string> Validate(PolicyAckOverride item)
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

    public void RequireValid(PolicyAckOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckOverride> items)
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

/// <summary>Functions of policy ack override rules.</summary>
public static class PolicyAckOverrideRules
{
    public static PolicyAckOverrideValidator DefaultPolicyAckOverrideValidator()
    {
        return new PolicyAckOverrideValidator(
            new List<IPolicyAckOverrideRule>
            {
                new PolicyAckOverrideCodeLengthRule(1000),
                new PolicyAckOverrideRequiredCodeRule("policy ack override needs an entry"),
                new PolicyAckOverrideRequiredNotesRule("policy ack override needs a value"),
                new PolicyAckOverrideRequiredLabelRule("policy ack override needs this field"),
                new PolicyAckOverrideCodePrefixRule("OPS"),
                new PolicyAckOverrideReviewerLengthRule(120),
            });
    }
}
