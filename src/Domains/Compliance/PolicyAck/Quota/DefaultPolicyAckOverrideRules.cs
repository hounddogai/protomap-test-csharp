namespace Acme.Workforce.Compliance.PolicyAck.Quota;

/// <summary>A rule a default policy ack override must pass. Check returns its problems, or none.</summary>
public interface IDefaultPolicyAckOverrideRule
{
    List<string> Check(DefaultPolicyAckOverride item);
}

public sealed record DefaultPolicyAckOverrideSummaryLengthRule(int MaxLength) : IDefaultPolicyAckOverrideRule
{
    public List<string> Check(DefaultPolicyAckOverride item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAckOverrideFteRangeRule(double Min, double Max) : IDefaultPolicyAckOverrideRule
{
    public List<string> Check(DefaultPolicyAckOverride item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultPolicyAckOverrideCodeLengthRule(int MaxLength) : IDefaultPolicyAckOverrideRule
{
    public List<string> Check(DefaultPolicyAckOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAckOverrideCodePrefixRule(string Prefix) : IDefaultPolicyAckOverrideRule
{
    public List<string> Check(DefaultPolicyAckOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyAckOverrideMinimumRangeRule(double Min, double Max) : IDefaultPolicyAckOverrideRule
{
    public List<string> Check(DefaultPolicyAckOverride item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default policy ack override.</summary>
public sealed record DefaultPolicyAckOverrideValidator(List<IDefaultPolicyAckOverrideRule> Rules)
{
    public List<string> Validate(DefaultPolicyAckOverride item)
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

    public void RequireValid(DefaultPolicyAckOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPolicyAckOverride> items)
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

/// <summary>Functions of default policy ack override rules.</summary>
public static class DefaultPolicyAckOverrideRules
{
    public static DefaultPolicyAckOverrideValidator DefaultDefaultPolicyAckOverrideValidator()
    {
        return new DefaultPolicyAckOverrideValidator(
            new List<IDefaultPolicyAckOverrideRule>
            {
                new DefaultPolicyAckOverrideSummaryLengthRule(64),
                new DefaultPolicyAckOverrideFteRangeRule(8.2, 181.1),
                new DefaultPolicyAckOverrideCodeLengthRule(255),
                new DefaultPolicyAckOverrideCodePrefixRule("X"),
                new DefaultPolicyAckOverrideMinimumRangeRule(5.6, 2757.5),
            });
    }
}
