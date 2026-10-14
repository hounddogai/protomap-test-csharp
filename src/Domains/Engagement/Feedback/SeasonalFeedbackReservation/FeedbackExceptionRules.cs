namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>A rule a feedback exception must pass. Check returns its problems, or none.</summary>
public interface IFeedbackExceptionRule
{
    List<string> Check(FeedbackException item);
}

public sealed record FeedbackExceptionCodeLengthRule(int MaxLength) : IFeedbackExceptionRule
{
    public List<string> Check(FeedbackException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackExceptionUpdatedAtRangeRule(int Min, int Max) : IFeedbackExceptionRule
{
    public List<string> Check(FeedbackException item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackExceptionCodePrefixRule(string Prefix) : IFeedbackExceptionRule
{
    public List<string> Check(FeedbackException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackExceptionRequiredSummaryRule(string Message) : IFeedbackExceptionRule
{
    public List<string> Check(FeedbackException item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback exception.</summary>
public sealed record FeedbackExceptionValidator(List<IFeedbackExceptionRule> Rules)
{
    public List<string> Validate(FeedbackException item)
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

    public void RequireValid(FeedbackException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackException> items)
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

/// <summary>Functions of feedback exception rules.</summary>
public static class FeedbackExceptionRules
{
    public static FeedbackExceptionValidator DefaultFeedbackExceptionValidator()
    {
        return new FeedbackExceptionValidator(
            new List<IFeedbackExceptionRule>
            {
                new FeedbackExceptionCodeLengthRule(1000),
                new FeedbackExceptionUpdatedAtRangeRule(32, 80),
                new FeedbackExceptionCodePrefixRule("OPS"),
                new FeedbackExceptionRequiredSummaryRule("feedback exception needs an entry"),
            });
    }
}
