namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>A rule a feedback threshold must pass. Check returns its problems, or none.</summary>
public interface IFeedbackThresholdRule
{
    List<string> Check(FeedbackThreshold item);
}

public sealed record FeedbackThresholdChannelLengthRule(int MaxLength) : IFeedbackThresholdRule
{
    public List<string> Check(FeedbackThreshold item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackThresholdPinnedGuardRule(int Limit) : IFeedbackThresholdRule
{
    public List<string> Check(FeedbackThreshold item)
    {
        if (item.Pinned && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackThresholdLabelLengthRule(int MaxLength) : IFeedbackThresholdRule
{
    public List<string> Check(FeedbackThreshold item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback threshold.</summary>
public sealed record FeedbackThresholdValidator(List<IFeedbackThresholdRule> Rules)
{
    public List<string> Validate(FeedbackThreshold item)
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

    public void RequireValid(FeedbackThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackThreshold> items)
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

/// <summary>Functions of feedback threshold rules.</summary>
public static class FeedbackThresholdRules
{
    public static FeedbackThresholdValidator DefaultFeedbackThresholdValidator()
    {
        return new FeedbackThresholdValidator(
            new List<IFeedbackThresholdRule>
            {
                new FeedbackThresholdChannelLengthRule(255),
                new FeedbackThresholdPinnedGuardRule(255),
                new FeedbackThresholdLabelLengthRule(255),
            });
    }
}
