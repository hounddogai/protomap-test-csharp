namespace Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>A rule a feedback window must pass. Check returns its problems, or none.</summary>
public interface IFeedbackWindowRule
{
    List<string> Check(FeedbackWindow item);
}

public sealed record FeedbackWindowBudgetRangeRule(double Min, double Max) : IFeedbackWindowRule
{
    public List<string> Check(FeedbackWindow item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackWindowCodeLengthRule(int MaxLength) : IFeedbackWindowRule
{
    public List<string> Check(FeedbackWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackWindowRequiredTitleRule(string Message) : IFeedbackWindowRule
{
    public List<string> Check(FeedbackWindow item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackWindowRequiredCodeRule(string Message) : IFeedbackWindowRule
{
    public List<string> Check(FeedbackWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackWindowQuantityRangeRule(int Min, int Max) : IFeedbackWindowRule
{
    public List<string> Check(FeedbackWindow item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a feedback window.</summary>
public sealed record FeedbackWindowValidator(List<IFeedbackWindowRule> Rules)
{
    public List<string> Validate(FeedbackWindow item)
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

    public void RequireValid(FeedbackWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackWindow> items)
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

/// <summary>Functions of feedback window rules.</summary>
public static class FeedbackWindowRules
{
    public static FeedbackWindowValidator DefaultFeedbackWindowValidator()
    {
        return new FeedbackWindowValidator(
            new List<IFeedbackWindowRule>
            {
                new FeedbackWindowBudgetRangeRule(6.7, 249.9),
                new FeedbackWindowCodeLengthRule(1000),
                new FeedbackWindowRequiredTitleRule("feedback window needs an entry"),
                new FeedbackWindowRequiredCodeRule("feedback window needs a value"),
                new FeedbackWindowQuantityRangeRule(32, 86400),
            });
    }
}
