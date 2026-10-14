namespace Acme.Workforce.Engagement.Feedback.Claim;

/// <summary>A rule a adaptive feedback score must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveFeedbackScoreRule
{
    List<string> Check(AdaptiveFeedbackScore item);
}

public sealed record AdaptiveFeedbackScoreArchivedGuardRule(double Limit) : IAdaptiveFeedbackScoreRule
{
    public List<string> Check(AdaptiveFeedbackScore item)
    {
        if (item.Archived && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFeedbackScoreTagsLimitRule(int MaxCount, List<string> Banned) : IAdaptiveFeedbackScoreRule
{
    public List<string> Check(AdaptiveFeedbackScore item)
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

public sealed record AdaptiveFeedbackScoreCostCenterLengthRule(int MaxLength) : IAdaptiveFeedbackScoreRule
{
    public List<string> Check(AdaptiveFeedbackScore item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFeedbackScoreCodePrefixRule(string Prefix) : IAdaptiveFeedbackScoreRule
{
    public List<string> Check(AdaptiveFeedbackScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveFeedbackScoreCodeLengthRule(int MaxLength) : IAdaptiveFeedbackScoreRule
{
    public List<string> Check(AdaptiveFeedbackScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive feedback score.</summary>
public sealed record AdaptiveFeedbackScoreValidator(List<IAdaptiveFeedbackScoreRule> Rules)
{
    public List<string> Validate(AdaptiveFeedbackScore item)
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

    public void RequireValid(AdaptiveFeedbackScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveFeedbackScore> items)
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

/// <summary>Functions of adaptive feedback score rules.</summary>
public static class AdaptiveFeedbackScoreRules
{
    public static AdaptiveFeedbackScoreValidator DefaultAdaptiveFeedbackScoreValidator()
    {
        return new AdaptiveFeedbackScoreValidator(
            new List<IAdaptiveFeedbackScoreRule>
            {
                new AdaptiveFeedbackScoreArchivedGuardRule(7.9),
                new AdaptiveFeedbackScoreTagsLimitRule(80, new List<string> { "lenient", "strict" }),
                new AdaptiveFeedbackScoreCostCenterLengthRule(64),
                new AdaptiveFeedbackScoreCodePrefixRule("REF"),
                new AdaptiveFeedbackScoreCodeLengthRule(86400),
            });
    }
}
