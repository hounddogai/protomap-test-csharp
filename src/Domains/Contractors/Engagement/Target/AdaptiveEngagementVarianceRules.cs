namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive engagement variance must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveEngagementVarianceRule
{
    List<string> Check(AdaptiveEngagementVariance item);
}

public sealed record AdaptiveEngagementVarianceRequiredLabelRule(string Message) : IAdaptiveEngagementVarianceRule
{
    public List<string> Check(AdaptiveEngagementVariance item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementVarianceAllowedSegmentRule(List<string> Allowed) : IAdaptiveEngagementVarianceRule
{
    public List<string> Check(AdaptiveEngagementVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementVarianceRequiredBadgeRule(string Message) : IAdaptiveEngagementVarianceRule
{
    public List<string> Check(AdaptiveEngagementVariance item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementVarianceRequiredCodeRule(string Message) : IAdaptiveEngagementVarianceRule
{
    public List<string> Check(AdaptiveEngagementVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementVarianceCodeLengthRule(int MaxLength) : IAdaptiveEngagementVarianceRule
{
    public List<string> Check(AdaptiveEngagementVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveEngagementVarianceLabelLengthRule(int MaxLength) : IAdaptiveEngagementVarianceRule
{
    public List<string> Check(AdaptiveEngagementVariance item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive engagement variance.</summary>
public sealed record AdaptiveEngagementVarianceValidator(List<IAdaptiveEngagementVarianceRule> Rules)
{
    public List<string> Validate(AdaptiveEngagementVariance item)
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

    public void RequireValid(AdaptiveEngagementVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveEngagementVariance> items)
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

/// <summary>Functions of adaptive engagement variance rules.</summary>
public static class AdaptiveEngagementVarianceRules
{
    public static AdaptiveEngagementVarianceValidator DefaultAdaptiveEngagementVarianceValidator()
    {
        return new AdaptiveEngagementVarianceValidator(
            new List<IAdaptiveEngagementVarianceRule>
            {
                new AdaptiveEngagementVarianceRequiredLabelRule("adaptive engagement variance needs a value"),
                new AdaptiveEngagementVarianceAllowedSegmentRule(
                    new List<string> { "weighted", "primary", "capped" }),
                new AdaptiveEngagementVarianceRequiredBadgeRule("adaptive engagement variance needs this field"),
                new AdaptiveEngagementVarianceRequiredCodeRule("adaptive engagement variance needs an entry"),
                new AdaptiveEngagementVarianceCodeLengthRule(3600),
                new AdaptiveEngagementVarianceLabelLengthRule(86400),
            });
    }
}
