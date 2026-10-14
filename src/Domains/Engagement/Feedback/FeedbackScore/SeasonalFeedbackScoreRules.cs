namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>A rule a seasonal feedback score must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFeedbackScoreRule
{
    List<string> Check(SeasonalFeedbackScore item);
}

public sealed record SeasonalFeedbackScoreRequiredCodeRule(string Message) : ISeasonalFeedbackScoreRule
{
    public List<string> Check(SeasonalFeedbackScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackScoreLocationLengthRule(int MaxLength) : ISeasonalFeedbackScoreRule
{
    public List<string> Check(SeasonalFeedbackScore item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackScoreTimezoneLengthRule(int MaxLength) : ISeasonalFeedbackScoreRule
{
    public List<string> Check(SeasonalFeedbackScore item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackScoreTitleLengthRule(int MaxLength) : ISeasonalFeedbackScoreRule
{
    public List<string> Check(SeasonalFeedbackScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal feedback score.</summary>
public sealed record SeasonalFeedbackScoreValidator(List<ISeasonalFeedbackScoreRule> Rules)
{
    public List<string> Validate(SeasonalFeedbackScore item)
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

    public void RequireValid(SeasonalFeedbackScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFeedbackScore> items)
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

/// <summary>Functions of seasonal feedback score rules.</summary>
public static class SeasonalFeedbackScoreRules
{
    public static SeasonalFeedbackScoreValidator DefaultSeasonalFeedbackScoreValidator()
    {
        return new SeasonalFeedbackScoreValidator(
            new List<ISeasonalFeedbackScoreRule>
            {
                new SeasonalFeedbackScoreRequiredCodeRule("seasonal feedback score needs this field"),
                new SeasonalFeedbackScoreLocationLengthRule(16),
                new SeasonalFeedbackScoreTimezoneLengthRule(255),
                new SeasonalFeedbackScoreTitleLengthRule(120),
            });
    }
}
