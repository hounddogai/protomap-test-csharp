namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive offboarding segment must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveOffboardingSegmentRule
{
    List<string> Check(AdaptiveOffboardingSegment item);
}

public sealed record AdaptiveOffboardingSegmentRequiredTitleRule(string Message) : IAdaptiveOffboardingSegmentRule
{
    public List<string> Check(AdaptiveOffboardingSegment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOffboardingSegmentRequiredCodeRule(string Message) : IAdaptiveOffboardingSegmentRule
{
    public List<string> Check(AdaptiveOffboardingSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOffboardingSegmentCodePrefixRule(string Prefix) : IAdaptiveOffboardingSegmentRule
{
    public List<string> Check(AdaptiveOffboardingSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOffboardingSegmentAllowedTitleRule(List<string> Allowed) : IAdaptiveOffboardingSegmentRule
{
    public List<string> Check(AdaptiveOffboardingSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveOffboardingSegmentTitleLengthRule(int MaxLength) : IAdaptiveOffboardingSegmentRule
{
    public List<string> Check(AdaptiveOffboardingSegment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive offboarding segment.</summary>
public sealed record AdaptiveOffboardingSegmentValidator(List<IAdaptiveOffboardingSegmentRule> Rules)
{
    public List<string> Validate(AdaptiveOffboardingSegment item)
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

    public void RequireValid(AdaptiveOffboardingSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveOffboardingSegment> items)
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

/// <summary>Functions of adaptive offboarding segment rules.</summary>
public static class AdaptiveOffboardingSegmentRules
{
    public static AdaptiveOffboardingSegmentValidator DefaultAdaptiveOffboardingSegmentValidator()
    {
        return new AdaptiveOffboardingSegmentValidator(
            new List<IAdaptiveOffboardingSegmentRule>
            {
                new AdaptiveOffboardingSegmentRequiredTitleRule("adaptive offboarding segment needs an entry"),
                new AdaptiveOffboardingSegmentRequiredCodeRule("adaptive offboarding segment needs a value"),
                new AdaptiveOffboardingSegmentCodePrefixRule("OPS"),
                new AdaptiveOffboardingSegmentAllowedTitleRule(new List<string> { "manual" }),
                new AdaptiveOffboardingSegmentTitleLengthRule(255),
            });
    }
}
