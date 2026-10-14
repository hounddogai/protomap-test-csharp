namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

/// <summary>A rule a lenient access badge estimate must pass. Check returns its problems, or none.</summary>
public interface ILenientAccessBadgeEstimateRule
{
    List<string> Check(LenientAccessBadgeEstimate item);
}

public sealed record LenientAccessBadgeEstimateRequiredLabelRule(string Message) : ILenientAccessBadgeEstimateRule
{
    public List<string> Check(LenientAccessBadgeEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientAccessBadgeEstimateLabelLengthRule(int MaxLength) : ILenientAccessBadgeEstimateRule
{
    public List<string> Check(LenientAccessBadgeEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAccessBadgeEstimateSourceLengthRule(int MaxLength) : ILenientAccessBadgeEstimateRule
{
    public List<string> Check(LenientAccessBadgeEstimate item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientAccessBadgeEstimateTagsLimitRule(int MaxCount, List<string> Banned) : ILenientAccessBadgeEstimateRule
{
    public List<string> Check(LenientAccessBadgeEstimate item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record LenientAccessBadgeEstimateRequiredSourceRule(string Message) : ILenientAccessBadgeEstimateRule
{
    public List<string> Check(LenientAccessBadgeEstimate item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient access badge estimate.</summary>
public sealed record LenientAccessBadgeEstimateValidator(List<ILenientAccessBadgeEstimateRule> Rules)
{
    public List<string> Validate(LenientAccessBadgeEstimate item)
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

    public void RequireValid(LenientAccessBadgeEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientAccessBadgeEstimate> items)
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

/// <summary>Functions of lenient access badge estimate rules.</summary>
public static class LenientAccessBadgeEstimateRules
{
    public static LenientAccessBadgeEstimateValidator DefaultLenientAccessBadgeEstimateValidator()
    {
        return new LenientAccessBadgeEstimateValidator(
            new List<ILenientAccessBadgeEstimateRule>
            {
                new LenientAccessBadgeEstimateRequiredLabelRule("lenient access badge estimate needs a value"),
                new LenientAccessBadgeEstimateLabelLengthRule(120),
                new LenientAccessBadgeEstimateSourceLengthRule(120),
                new LenientAccessBadgeEstimateTagsLimitRule(255, new List<string> { "lenient", "default" }),
                new LenientAccessBadgeEstimateRequiredSourceRule("lenient access badge estimate needs an entry"),
            });
    }
}
