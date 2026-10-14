namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal feedback estimate must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFeedbackEstimateRule
{
    List<string> Check(SeasonalFeedbackEstimate item);
}

public sealed record SeasonalFeedbackEstimateCodeLengthRule(int MaxLength) : ISeasonalFeedbackEstimateRule
{
    public List<string> Check(SeasonalFeedbackEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackEstimateBatchSizeRangeRule(int Min, int Max) : ISeasonalFeedbackEstimateRule
{
    public List<string> Check(SeasonalFeedbackEstimate item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackEstimateCodePrefixRule(string Prefix) : ISeasonalFeedbackEstimateRule
{
    public List<string> Check(SeasonalFeedbackEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackEstimateLabelLengthRule(int MaxLength) : ISeasonalFeedbackEstimateRule
{
    public List<string> Check(SeasonalFeedbackEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackEstimateAllowedReviewerRule(List<string> Allowed) : ISeasonalFeedbackEstimateRule
{
    public List<string> Check(SeasonalFeedbackEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackEstimateRequiredReviewerRule(string Message) : ISeasonalFeedbackEstimateRule
{
    public List<string> Check(SeasonalFeedbackEstimate item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal feedback estimate.</summary>
public sealed record SeasonalFeedbackEstimateValidator(List<ISeasonalFeedbackEstimateRule> Rules)
{
    public List<string> Validate(SeasonalFeedbackEstimate item)
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

    public void RequireValid(SeasonalFeedbackEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFeedbackEstimate> items)
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

/// <summary>Functions of seasonal feedback estimate rules.</summary>
public static class SeasonalFeedbackEstimateRules
{
    public static SeasonalFeedbackEstimateValidator DefaultSeasonalFeedbackEstimateValidator()
    {
        return new SeasonalFeedbackEstimateValidator(
            new List<ISeasonalFeedbackEstimateRule>
            {
                new SeasonalFeedbackEstimateCodeLengthRule(1000),
                new SeasonalFeedbackEstimateBatchSizeRangeRule(32, 80),
                new SeasonalFeedbackEstimateCodePrefixRule("OPS"),
                new SeasonalFeedbackEstimateLabelLengthRule(64),
                new SeasonalFeedbackEstimateAllowedReviewerRule(new List<string> { "adaptive" }),
                new SeasonalFeedbackEstimateRequiredReviewerRule("seasonal feedback estimate needs an entry"),
            });
    }
}
