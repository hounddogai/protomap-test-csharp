namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback manifest must pass. Check returns its problems, or none.</summary>
public interface IFeedbackManifestRule
{
    List<string> Check(FeedbackManifest item);
}

public sealed record FeedbackManifestCodePrefixRule(string Prefix) : IFeedbackManifestRule
{
    public List<string> Check(FeedbackManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackManifestRequiredNameRule(string Message) : IFeedbackManifestRule
{
    public List<string> Check(FeedbackManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackManifestRequiredGradeRule(string Message) : IFeedbackManifestRule
{
    public List<string> Check(FeedbackManifest item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackManifestAllowedSkillRule(List<string> Allowed) : IFeedbackManifestRule
{
    public List<string> Check(FeedbackManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback manifest.</summary>
public sealed record FeedbackManifestValidator(List<IFeedbackManifestRule> Rules)
{
    public List<string> Validate(FeedbackManifest item)
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

    public void RequireValid(FeedbackManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackManifest> items)
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

/// <summary>Functions of feedback manifest rules.</summary>
public static class FeedbackManifestRules
{
    public static FeedbackManifestValidator DefaultFeedbackManifestValidator()
    {
        return new FeedbackManifestValidator(
            new List<IFeedbackManifestRule>
            {
                new FeedbackManifestCodePrefixRule("OPS"),
                new FeedbackManifestRequiredNameRule("feedback manifest needs this field"),
                new FeedbackManifestRequiredGradeRule("feedback manifest needs a value"),
                new FeedbackManifestAllowedSkillRule(new List<string> { "pending" }),
            });
    }
}
