namespace Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>A rule a feedback forecast must pass. Check returns its problems, or none.</summary>
public interface IFeedbackForecastRule
{
    List<string> Check(FeedbackForecast item);
}

public sealed record FeedbackForecastRequiredCodeRule(string Message) : IFeedbackForecastRule
{
    public List<string> Check(FeedbackForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackForecastCodeLengthRule(int MaxLength) : IFeedbackForecastRule
{
    public List<string> Check(FeedbackForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackForecastRequiredLabelRule(string Message) : IFeedbackForecastRule
{
    public List<string> Check(FeedbackForecast item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackForecastRequiredDescriptionRule(string Message) : IFeedbackForecastRule
{
    public List<string> Check(FeedbackForecast item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackForecastLabelLengthRule(int MaxLength) : IFeedbackForecastRule
{
    public List<string> Check(FeedbackForecast item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback forecast.</summary>
public sealed record FeedbackForecastValidator(List<IFeedbackForecastRule> Rules)
{
    public List<string> Validate(FeedbackForecast item)
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

    public void RequireValid(FeedbackForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackForecast> items)
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

/// <summary>Functions of feedback forecast rules.</summary>
public static class FeedbackForecastRules
{
    public static FeedbackForecastValidator DefaultFeedbackForecastValidator()
    {
        return new FeedbackForecastValidator(
            new List<IFeedbackForecastRule>
            {
                new FeedbackForecastRequiredCodeRule("feedback forecast needs this field"),
                new FeedbackForecastCodeLengthRule(86400),
                new FeedbackForecastRequiredLabelRule("feedback forecast needs an entry"),
                new FeedbackForecastRequiredDescriptionRule("feedback forecast needs a value"),
                new FeedbackForecastLabelLengthRule(120),
            });
    }
}
