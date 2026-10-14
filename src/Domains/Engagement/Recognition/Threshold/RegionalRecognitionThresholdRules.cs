namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional recognition threshold must pass. Check returns its problems, or none.</summary>
public interface IRegionalRecognitionThresholdRule
{
    List<string> Check(RegionalRecognitionThreshold item);
}

public sealed record RegionalRecognitionThresholdTagsLimitRule(int MaxCount, List<string> Banned) : IRegionalRecognitionThresholdRule
{
    public List<string> Check(RegionalRecognitionThreshold item)
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

public sealed record RegionalRecognitionThresholdAllowedDescriptionRule(List<string> Allowed) : IRegionalRecognitionThresholdRule
{
    public List<string> Check(RegionalRecognitionThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RegionalRecognitionThresholdDescriptionLengthRule(int MaxLength) : IRegionalRecognitionThresholdRule
{
    public List<string> Check(RegionalRecognitionThreshold item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalRecognitionThresholdRequiredCodeRule(string Message) : IRegionalRecognitionThresholdRule
{
    public List<string> Check(RegionalRecognitionThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional recognition threshold.</summary>
public sealed record RegionalRecognitionThresholdValidator(List<IRegionalRecognitionThresholdRule> Rules)
{
    public List<string> Validate(RegionalRecognitionThreshold item)
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

    public void RequireValid(RegionalRecognitionThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalRecognitionThreshold> items)
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

/// <summary>Functions of regional recognition threshold rules.</summary>
public static class RegionalRecognitionThresholdRules
{
    public static RegionalRecognitionThresholdValidator DefaultRegionalRecognitionThresholdValidator()
    {
        return new RegionalRecognitionThresholdValidator(
            new List<IRegionalRecognitionThresholdRule>
            {
                new RegionalRecognitionThresholdTagsLimitRule(
                    64,
                    new List<string> { "capped", "scheduled", "legacy" }),
                new RegionalRecognitionThresholdAllowedDescriptionRule(
                    new List<string> { "legacy", "lenient", "rolling" }),
                new RegionalRecognitionThresholdDescriptionLengthRule(32),
                new RegionalRecognitionThresholdRequiredCodeRule("regional recognition threshold needs an entry"),
            });
    }
}
