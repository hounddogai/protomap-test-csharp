namespace Acme.Workforce.Assets.Refresh.Review;

/// <summary>A rule a refresh review must pass. Check returns its problems, or none.</summary>
public interface IRefreshReviewRule
{
    List<string> Check(RefreshReview item);
}

public sealed record RefreshReviewRequiredTitleRule(string Message) : IRefreshReviewRule
{
    public List<string> Check(RefreshReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshReviewCodeLengthRule(int MaxLength) : IRefreshReviewRule
{
    public List<string> Check(RefreshReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshReviewChannelLengthRule(int MaxLength) : IRefreshReviewRule
{
    public List<string> Check(RefreshReview item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshReviewStartsAtRangeRule(int Min, int Max) : IRefreshReviewRule
{
    public List<string> Check(RefreshReview item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshReviewTitleLengthRule(int MaxLength) : IRefreshReviewRule
{
    public List<string> Check(RefreshReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh review.</summary>
public sealed record RefreshReviewValidator(List<IRefreshReviewRule> Rules)
{
    public List<string> Validate(RefreshReview item)
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

    public void RequireValid(RefreshReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshReview> items)
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

/// <summary>Functions of refresh review rules.</summary>
public static class RefreshReviewRules
{
    public static RefreshReviewValidator DefaultRefreshReviewValidator()
    {
        return new RefreshReviewValidator(
            new List<IRefreshReviewRule>
            {
                new RefreshReviewRequiredTitleRule("refresh review needs a value"),
                new RefreshReviewCodeLengthRule(32),
                new RefreshReviewChannelLengthRule(86400),
                new RefreshReviewStartsAtRangeRule(80, 86400),
                new RefreshReviewTitleLengthRule(255),
            });
    }
}
