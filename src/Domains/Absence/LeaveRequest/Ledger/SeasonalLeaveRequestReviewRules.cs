namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>A rule a seasonal leave request review must pass. Check returns its problems, or none.</summary>
public interface ISeasonalLeaveRequestReviewRule
{
    List<string> Check(SeasonalLeaveRequestReview item);
}

public sealed record SeasonalLeaveRequestReviewRequiredTitleRule(string Message) : ISeasonalLeaveRequestReviewRule
{
    public List<string> Check(SeasonalLeaveRequestReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLeaveRequestReviewBadgeLengthRule(int MaxLength) : ISeasonalLeaveRequestReviewRule
{
    public List<string> Check(SeasonalLeaveRequestReview item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLeaveRequestReviewTitleLengthRule(int MaxLength) : ISeasonalLeaveRequestReviewRule
{
    public List<string> Check(SeasonalLeaveRequestReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal leave request review.</summary>
public sealed record SeasonalLeaveRequestReviewValidator(List<ISeasonalLeaveRequestReviewRule> Rules)
{
    public List<string> Validate(SeasonalLeaveRequestReview item)
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

    public void RequireValid(SeasonalLeaveRequestReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalLeaveRequestReview> items)
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

/// <summary>Functions of seasonal leave request review rules.</summary>
public static class SeasonalLeaveRequestReviewRules
{
    public static SeasonalLeaveRequestReviewValidator DefaultSeasonalLeaveRequestReviewValidator()
    {
        return new SeasonalLeaveRequestReviewValidator(
            new List<ISeasonalLeaveRequestReviewRule>
            {
                new SeasonalLeaveRequestReviewRequiredTitleRule("seasonal leave request review needs a value"),
                new SeasonalLeaveRequestReviewBadgeLengthRule(86400),
                new SeasonalLeaveRequestReviewTitleLengthRule(1000),
            });
    }
}
