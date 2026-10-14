namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

/// <summary>A rule a seasonal feedback window must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFeedbackWindowRule
{
    List<string> Check(SeasonalFeedbackWindow item);
}

public sealed record SeasonalFeedbackWindowRequiredNameRule(string Message) : ISeasonalFeedbackWindowRule
{
    public List<string> Check(SeasonalFeedbackWindow item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackWindowSequenceRangeRule(int Min, int Max) : ISeasonalFeedbackWindowRule
{
    public List<string> Check(SeasonalFeedbackWindow item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalFeedbackWindowCodeLengthRule(int MaxLength) : ISeasonalFeedbackWindowRule
{
    public List<string> Check(SeasonalFeedbackWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackWindowRequiredCurrencyRule(string Message) : ISeasonalFeedbackWindowRule
{
    public List<string> Check(SeasonalFeedbackWindow item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackWindowLeaveDaysRangeRule(int Min, int Max) : ISeasonalFeedbackWindowRule
{
    public List<string> Check(SeasonalFeedbackWindow item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalFeedbackWindowRequiredCountryRule(string Message) : ISeasonalFeedbackWindowRule
{
    public List<string> Check(SeasonalFeedbackWindow item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal feedback window.</summary>
public sealed record SeasonalFeedbackWindowValidator(List<ISeasonalFeedbackWindowRule> Rules)
{
    public List<string> Validate(SeasonalFeedbackWindow item)
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

    public void RequireValid(SeasonalFeedbackWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFeedbackWindow> items)
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

/// <summary>Functions of seasonal feedback window rules.</summary>
public static class SeasonalFeedbackWindowRules
{
    public static SeasonalFeedbackWindowValidator DefaultSeasonalFeedbackWindowValidator()
    {
        return new SeasonalFeedbackWindowValidator(
            new List<ISeasonalFeedbackWindowRule>
            {
                new SeasonalFeedbackWindowRequiredNameRule("seasonal feedback window needs a value"),
                new SeasonalFeedbackWindowSequenceRangeRule(16, 80),
                new SeasonalFeedbackWindowCodeLengthRule(64),
                new SeasonalFeedbackWindowRequiredCurrencyRule("seasonal feedback window needs an entry"),
                new SeasonalFeedbackWindowLeaveDaysRangeRule(16, 86400),
                new SeasonalFeedbackWindowRequiredCountryRule("seasonal feedback window needs an entry"),
            });
    }
}
