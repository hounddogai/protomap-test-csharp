namespace Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>A rule a engagement manifest must pass. Check returns its problems, or none.</summary>
public interface IEngagementManifestRule
{
    List<string> Check(EngagementManifest item);
}

public sealed record EngagementManifestCodeLengthRule(int MaxLength) : IEngagementManifestRule
{
    public List<string> Check(EngagementManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementManifestCodePrefixRule(string Prefix) : IEngagementManifestRule
{
    public List<string> Check(EngagementManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementManifestRequiredTitleRule(string Message) : IEngagementManifestRule
{
    public List<string> Check(EngagementManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement manifest.</summary>
public sealed record EngagementManifestValidator(List<IEngagementManifestRule> Rules)
{
    public List<string> Validate(EngagementManifest item)
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

    public void RequireValid(EngagementManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementManifest> items)
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

/// <summary>Functions of engagement manifest rules.</summary>
public static class EngagementManifestRules
{
    public static EngagementManifestValidator DefaultEngagementManifestValidator()
    {
        return new EngagementManifestValidator(
            new List<IEngagementManifestRule>
            {
                new EngagementManifestCodeLengthRule(3600),
                new EngagementManifestCodePrefixRule("ACM"),
                new EngagementManifestRequiredTitleRule("engagement manifest needs an entry"),
            });
    }
}
