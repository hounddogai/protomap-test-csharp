namespace Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>A rule a policy ack draft must pass. Check returns its problems, or none.</summary>
public interface IPolicyAckDraftRule
{
    List<string> Check(PolicyAckDraft item);
}

public sealed record PolicyAckDraftTimezoneLengthRule(int MaxLength) : IPolicyAckDraftRule
{
    public List<string> Check(PolicyAckDraft item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckDraftTitleLengthRule(int MaxLength) : IPolicyAckDraftRule
{
    public List<string> Check(PolicyAckDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckDraftCodeLengthRule(int MaxLength) : IPolicyAckDraftRule
{
    public List<string> Check(PolicyAckDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckDraftRequiredTitleRule(string Message) : IPolicyAckDraftRule
{
    public List<string> Check(PolicyAckDraft item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyAckDraftDurationMinutesRangeRule(int Min, int Max) : IPolicyAckDraftRule
{
    public List<string> Check(PolicyAckDraft item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PolicyAckDraftCodePrefixRule(string Prefix) : IPolicyAckDraftRule
{
    public List<string> Check(PolicyAckDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy ack draft.</summary>
public sealed record PolicyAckDraftValidator(List<IPolicyAckDraftRule> Rules)
{
    public List<string> Validate(PolicyAckDraft item)
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

    public void RequireValid(PolicyAckDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyAckDraft> items)
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

/// <summary>Functions of policy ack draft rules.</summary>
public static class PolicyAckDraftRules
{
    public static PolicyAckDraftValidator DefaultPolicyAckDraftValidator()
    {
        return new PolicyAckDraftValidator(
            new List<IPolicyAckDraftRule>
            {
                new PolicyAckDraftTimezoneLengthRule(120),
                new PolicyAckDraftTitleLengthRule(80),
                new PolicyAckDraftCodeLengthRule(120),
                new PolicyAckDraftRequiredTitleRule("policy ack draft needs a value"),
                new PolicyAckDraftDurationMinutesRangeRule(64, 86400),
                new PolicyAckDraftCodePrefixRule("ACM"),
            });
    }
}
