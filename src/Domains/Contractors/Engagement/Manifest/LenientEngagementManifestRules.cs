namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient engagement manifest must pass. Check returns its problems, or none.</summary>
public interface ILenientEngagementManifestRule
{
    List<string> Check(LenientEngagementManifest item);
}

public sealed record LenientEngagementManifestRequiredChannelRule(string Message) : ILenientEngagementManifestRule
{
    public List<string> Check(LenientEngagementManifest item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientEngagementManifestAllowedUnitRule(List<string> Allowed) : ILenientEngagementManifestRule
{
    public List<string> Check(LenientEngagementManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LenientEngagementManifestRequiredLabelRule(string Message) : ILenientEngagementManifestRule
{
    public List<string> Check(LenientEngagementManifest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient engagement manifest.</summary>
public sealed record LenientEngagementManifestValidator(List<ILenientEngagementManifestRule> Rules)
{
    public List<string> Validate(LenientEngagementManifest item)
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

    public void RequireValid(LenientEngagementManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientEngagementManifest> items)
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

/// <summary>Functions of lenient engagement manifest rules.</summary>
public static class LenientEngagementManifestRules
{
    public static LenientEngagementManifestValidator DefaultLenientEngagementManifestValidator()
    {
        return new LenientEngagementManifestValidator(
            new List<ILenientEngagementManifestRule>
            {
                new LenientEngagementManifestRequiredChannelRule("lenient engagement manifest needs a value"),
                new LenientEngagementManifestAllowedUnitRule(new List<string> { "legacy" }),
                new LenientEngagementManifestRequiredLabelRule("lenient engagement manifest needs this field"),
            });
    }
}
