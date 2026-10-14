namespace Acme.Workforce.Compliance.Exception.Review;

/// <summary>A rule a exception review must pass. Check returns its problems, or none.</summary>
public interface IExceptionReviewRule
{
    List<string> Check(ExceptionReview item);
}

public sealed record ExceptionReviewCreatedAtRangeRule(int Min, int Max) : IExceptionReviewRule
{
    public List<string> Check(ExceptionReview item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionReviewRequiredCodeRule(string Message) : IExceptionReviewRule
{
    public List<string> Check(ExceptionReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionReviewTitleLengthRule(int MaxLength) : IExceptionReviewRule
{
    public List<string> Check(ExceptionReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionReviewLocaleLengthRule(int MaxLength) : IExceptionReviewRule
{
    public List<string> Check(ExceptionReview item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionReviewScoreRangeRule(double Min, double Max) : IExceptionReviewRule
{
    public List<string> Check(ExceptionReview item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception review.</summary>
public sealed record ExceptionReviewValidator(List<IExceptionReviewRule> Rules)
{
    public List<string> Validate(ExceptionReview item)
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

    public void RequireValid(ExceptionReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionReview> items)
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

/// <summary>Functions of exception review rules.</summary>
public static class ExceptionReviewRules
{
    public static ExceptionReviewValidator DefaultExceptionReviewValidator()
    {
        return new ExceptionReviewValidator(
            new List<IExceptionReviewRule>
            {
                new ExceptionReviewCreatedAtRangeRule(16, 80),
                new ExceptionReviewRequiredCodeRule("exception review needs an entry"),
                new ExceptionReviewTitleLengthRule(86400),
                new ExceptionReviewLocaleLengthRule(16),
                new ExceptionReviewScoreRangeRule(2.1, 2397.9),
            });
    }
}
