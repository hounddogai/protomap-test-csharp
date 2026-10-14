namespace Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>A rule a feedback baseline must pass. Check returns its problems, or none.</summary>
public interface IFeedbackBaselineRule
{
    List<string> Check(FeedbackBaseline item);
}

public sealed record FeedbackBaselineTitleLengthRule(int MaxLength) : IFeedbackBaselineRule
{
    public List<string> Check(FeedbackBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackBaselineSummaryLengthRule(int MaxLength) : IFeedbackBaselineRule
{
    public List<string> Check(FeedbackBaseline item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackBaselineTeamLengthRule(int MaxLength) : IFeedbackBaselineRule
{
    public List<string> Check(FeedbackBaseline item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback baseline.</summary>
public sealed record FeedbackBaselineValidator(List<IFeedbackBaselineRule> Rules)
{
    public List<string> Validate(FeedbackBaseline item)
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

    public void RequireValid(FeedbackBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackBaseline> items)
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

/// <summary>Functions of feedback baseline rules.</summary>
public static class FeedbackBaselineRules
{
    public static FeedbackBaselineValidator DefaultFeedbackBaselineValidator()
    {
        return new FeedbackBaselineValidator(
            new List<IFeedbackBaselineRule>
            {
                new FeedbackBaselineTitleLengthRule(1000),
                new FeedbackBaselineSummaryLengthRule(255),
                new FeedbackBaselineTeamLengthRule(120),
            });
    }
}
