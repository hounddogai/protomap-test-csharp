namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback checkpoint must pass. Check returns its problems, or none.</summary>
public interface IFeedbackCheckpointRule
{
    List<string> Check(FeedbackCheckpoint item);
}

public sealed record FeedbackCheckpointNameLengthRule(int MaxLength) : IFeedbackCheckpointRule
{
    public List<string> Check(FeedbackCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackCheckpointRequiredLocaleRule(string Message) : IFeedbackCheckpointRule
{
    public List<string> Check(FeedbackCheckpoint item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackCheckpointAllowedCurrencyRule(List<string> Allowed) : IFeedbackCheckpointRule
{
    public List<string> Check(FeedbackCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback checkpoint.</summary>
public sealed record FeedbackCheckpointValidator(List<IFeedbackCheckpointRule> Rules)
{
    public List<string> Validate(FeedbackCheckpoint item)
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

    public void RequireValid(FeedbackCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackCheckpoint> items)
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

/// <summary>Functions of feedback checkpoint rules.</summary>
public static class FeedbackCheckpointRules
{
    public static FeedbackCheckpointValidator DefaultFeedbackCheckpointValidator()
    {
        return new FeedbackCheckpointValidator(
            new List<IFeedbackCheckpointRule>
            {
                new FeedbackCheckpointNameLengthRule(120),
                new FeedbackCheckpointRequiredLocaleRule("feedback checkpoint needs a value"),
                new FeedbackCheckpointAllowedCurrencyRule(new List<string> { "capped" }),
            });
    }
}
