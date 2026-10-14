namespace Acme.Workforce.Workplace.Desk.Policy;

/// <summary>A rule a default desk policy must pass. Check returns its problems, or none.</summary>
public interface IDefaultDeskPolicyRule
{
    List<string> Check(DefaultDeskPolicy item);
}

public sealed record DefaultDeskPolicyCodeLengthRule(int MaxLength) : IDefaultDeskPolicyRule
{
    public List<string> Check(DefaultDeskPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultDeskPolicyRequiredCodeRule(string Message) : IDefaultDeskPolicyRule
{
    public List<string> Check(DefaultDeskPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultDeskPolicyCodePrefixRule(string Prefix) : IDefaultDeskPolicyRule
{
    public List<string> Check(DefaultDeskPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultDeskPolicyReviewerLengthRule(int MaxLength) : IDefaultDeskPolicyRule
{
    public List<string> Check(DefaultDeskPolicy item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default desk policy.</summary>
public sealed record DefaultDeskPolicyValidator(List<IDefaultDeskPolicyRule> Rules)
{
    public List<string> Validate(DefaultDeskPolicy item)
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

    public void RequireValid(DefaultDeskPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultDeskPolicy> items)
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

/// <summary>Functions of default desk policy rules.</summary>
public static class DefaultDeskPolicyRules
{
    public static DefaultDeskPolicyValidator DefaultDefaultDeskPolicyValidator()
    {
        return new DefaultDeskPolicyValidator(
            new List<IDefaultDeskPolicyRule>
            {
                new DefaultDeskPolicyCodeLengthRule(64),
                new DefaultDeskPolicyRequiredCodeRule("default desk policy needs a value"),
                new DefaultDeskPolicyCodePrefixRule("ACM"),
                new DefaultDeskPolicyReviewerLengthRule(3600),
            });
    }
}
