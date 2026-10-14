namespace Acme.Workforce.Contractors.Engagement.Score;

/// <summary>A rule a engagement digest must pass. Check returns its problems, or none.</summary>
public interface IEngagementDigestRule
{
    List<string> Check(EngagementDigest item);
}

public sealed record EngagementDigestCodePrefixRule(string Prefix) : IEngagementDigestRule
{
    public List<string> Check(EngagementDigest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementDigestRequiredNameRule(string Message) : IEngagementDigestRule
{
    public List<string> Check(EngagementDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementDigestCostCenterLengthRule(int MaxLength) : IEngagementDigestRule
{
    public List<string> Check(EngagementDigest item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementDigestCodeLengthRule(int MaxLength) : IEngagementDigestRule
{
    public List<string> Check(EngagementDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement digest.</summary>
public sealed record EngagementDigestValidator(List<IEngagementDigestRule> Rules)
{
    public List<string> Validate(EngagementDigest item)
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

    public void RequireValid(EngagementDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementDigest> items)
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

/// <summary>Functions of engagement digest rules.</summary>
public static class EngagementDigestRules
{
    public static EngagementDigestValidator DefaultEngagementDigestValidator()
    {
        return new EngagementDigestValidator(
            new List<IEngagementDigestRule>
            {
                new EngagementDigestCodePrefixRule("X"),
                new EngagementDigestRequiredNameRule("engagement digest needs a value"),
                new EngagementDigestCostCenterLengthRule(1000),
                new EngagementDigestCodeLengthRule(3600),
            });
    }
}
