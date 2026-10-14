namespace Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>A rule a access badge policy must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgePolicyRule
{
    List<string> Check(AccessBadgePolicy item);
}

public sealed record AccessBadgePolicyRequiredCodeRule(string Message) : IAccessBadgePolicyRule
{
    public List<string> Check(AccessBadgePolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePolicyCodeLengthRule(int MaxLength) : IAccessBadgePolicyRule
{
    public List<string> Check(AccessBadgePolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePolicyCodePrefixRule(string Prefix) : IAccessBadgePolicyRule
{
    public List<string> Check(AccessBadgePolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePolicyDescriptionLengthRule(int MaxLength) : IAccessBadgePolicyRule
{
    public List<string> Check(AccessBadgePolicy item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge policy.</summary>
public sealed record AccessBadgePolicyValidator(List<IAccessBadgePolicyRule> Rules)
{
    public List<string> Validate(AccessBadgePolicy item)
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

    public void RequireValid(AccessBadgePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgePolicy> items)
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

/// <summary>Functions of access badge policy rules.</summary>
public static class AccessBadgePolicyRules
{
    public static AccessBadgePolicyValidator DefaultAccessBadgePolicyValidator()
    {
        return new AccessBadgePolicyValidator(
            new List<IAccessBadgePolicyRule>
            {
                new AccessBadgePolicyRequiredCodeRule("access badge policy needs this field"),
                new AccessBadgePolicyCodeLengthRule(16),
                new AccessBadgePolicyCodePrefixRule("Q"),
                new AccessBadgePolicyDescriptionLengthRule(86400),
            });
    }
}
