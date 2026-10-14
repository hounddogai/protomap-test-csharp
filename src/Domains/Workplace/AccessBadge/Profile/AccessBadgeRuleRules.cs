namespace Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>A rule a access badge rule must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeRuleRule
{
    List<string> Check(AccessBadgeRule item);
}

public sealed record AccessBadgeRuleLabelLengthRule(int MaxLength) : IAccessBadgeRuleRule
{
    public List<string> Check(AccessBadgeRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeRulePhoneLengthRule(int MaxLength) : IAccessBadgeRuleRule
{
    public List<string> Check(AccessBadgeRule item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeRuleCodeLengthRule(int MaxLength) : IAccessBadgeRuleRule
{
    public List<string> Check(AccessBadgeRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeRuleCodePrefixRule(string Prefix) : IAccessBadgeRuleRule
{
    public List<string> Check(AccessBadgeRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeRuleRequiredCodeRule(string Message) : IAccessBadgeRuleRule
{
    public List<string> Check(AccessBadgeRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeRuleRequiredChannelRule(string Message) : IAccessBadgeRuleRule
{
    public List<string> Check(AccessBadgeRule item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge rule.</summary>
public sealed record AccessBadgeRuleValidator(List<IAccessBadgeRuleRule> Rules)
{
    public List<string> Validate(AccessBadgeRule item)
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

    public void RequireValid(AccessBadgeRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeRule> items)
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

/// <summary>Functions of access badge rule rules.</summary>
public static class AccessBadgeRuleRules
{
    public static AccessBadgeRuleValidator DefaultAccessBadgeRuleValidator()
    {
        return new AccessBadgeRuleValidator(
            new List<IAccessBadgeRuleRule>
            {
                new AccessBadgeRuleLabelLengthRule(255),
                new AccessBadgeRulePhoneLengthRule(120),
                new AccessBadgeRuleCodeLengthRule(86400),
                new AccessBadgeRuleCodePrefixRule("Q"),
                new AccessBadgeRuleRequiredCodeRule("access badge rule needs an entry"),
                new AccessBadgeRuleRequiredChannelRule("access badge rule needs a value"),
            });
    }
}
