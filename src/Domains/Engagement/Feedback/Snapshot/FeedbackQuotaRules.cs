namespace Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>A rule a feedback quota must pass. Check returns its problems, or none.</summary>
public interface IFeedbackQuotaRule
{
    List<string> Check(FeedbackQuota item);
}

public sealed record FeedbackQuotaSalaryBandLengthRule(int MaxLength) : IFeedbackQuotaRule
{
    public List<string> Check(FeedbackQuota item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackQuotaRegionLengthRule(int MaxLength) : IFeedbackQuotaRule
{
    public List<string> Check(FeedbackQuota item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackQuotaRequiredRegionRule(string Message) : IFeedbackQuotaRule
{
    public List<string> Check(FeedbackQuota item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackQuotaRatioRangeRule(double Min, double Max) : IFeedbackQuotaRule
{
    public List<string> Check(FeedbackQuota item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a feedback quota.</summary>
public sealed record FeedbackQuotaValidator(List<IFeedbackQuotaRule> Rules)
{
    public List<string> Validate(FeedbackQuota item)
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

    public void RequireValid(FeedbackQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackQuota> items)
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

/// <summary>Functions of feedback quota rules.</summary>
public static class FeedbackQuotaRules
{
    public static FeedbackQuotaValidator DefaultFeedbackQuotaValidator()
    {
        return new FeedbackQuotaValidator(
            new List<IFeedbackQuotaRule>
            {
                new FeedbackQuotaSalaryBandLengthRule(64),
                new FeedbackQuotaRegionLengthRule(120),
                new FeedbackQuotaRequiredRegionRule("feedback quota needs a value"),
                new FeedbackQuotaRatioRangeRule(8.1, 548.1),
            });
    }
}
