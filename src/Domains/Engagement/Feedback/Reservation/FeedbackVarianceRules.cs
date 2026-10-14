namespace Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>A rule a feedback variance must pass. Check returns its problems, or none.</summary>
public interface IFeedbackVarianceRule
{
    List<string> Check(FeedbackVariance item);
}

public sealed record FeedbackVarianceCostRangeRule(double Min, double Max) : IFeedbackVarianceRule
{
    public List<string> Check(FeedbackVariance item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FeedbackVarianceRequiredLocaleRule(string Message) : IFeedbackVarianceRule
{
    public List<string> Check(FeedbackVariance item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackVarianceLocaleLengthRule(int MaxLength) : IFeedbackVarianceRule
{
    public List<string> Check(FeedbackVariance item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackVarianceCodeLengthRule(int MaxLength) : IFeedbackVarianceRule
{
    public List<string> Check(FeedbackVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback variance.</summary>
public sealed record FeedbackVarianceValidator(List<IFeedbackVarianceRule> Rules)
{
    public List<string> Validate(FeedbackVariance item)
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

    public void RequireValid(FeedbackVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackVariance> items)
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

/// <summary>Functions of feedback variance rules.</summary>
public static class FeedbackVarianceRules
{
    public static FeedbackVarianceValidator DefaultFeedbackVarianceValidator()
    {
        return new FeedbackVarianceValidator(
            new List<IFeedbackVarianceRule>
            {
                new FeedbackVarianceCostRangeRule(4.7, 2876.2),
                new FeedbackVarianceRequiredLocaleRule("feedback variance needs an entry"),
                new FeedbackVarianceLocaleLengthRule(1000),
                new FeedbackVarianceCodeLengthRule(3600),
            });
    }
}
