namespace Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>A rule a policy override must pass. Check returns its problems, or none.</summary>
public interface IPolicyOverrideRule
{
    List<string> Check(PolicyOverride item);
}

public sealed record PolicyOverrideRequiredNameRule(string Message) : IPolicyOverrideRule
{
    public List<string> Check(PolicyOverride item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyOverrideCodePrefixRule(string Prefix) : IPolicyOverrideRule
{
    public List<string> Check(PolicyOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyOverrideScoreRangeRule(double Min, double Max) : IPolicyOverrideRule
{
    public List<string> Check(PolicyOverride item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyOverrideSequenceRangeRule(int Min, int Max) : IPolicyOverrideRule
{
    public List<string> Check(PolicyOverride item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy override.</summary>
public sealed record PolicyOverrideValidator(List<IPolicyOverrideRule> Rules)
{
    public List<string> Validate(PolicyOverride item)
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

    public void RequireValid(PolicyOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyOverride> items)
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

/// <summary>Functions of policy override rules.</summary>
public static class PolicyOverrideRules
{
    public static PolicyOverrideValidator DefaultPolicyOverrideValidator()
    {
        return new PolicyOverrideValidator(
            new List<IPolicyOverrideRule>
            {
                new PolicyOverrideRequiredNameRule("policy override needs a value"),
                new PolicyOverrideCodePrefixRule("REF"),
                new PolicyOverrideScoreRangeRule(3.1, 2812.3),
                new PolicyOverrideSequenceRangeRule(120, 3600),
            });
    }
}
