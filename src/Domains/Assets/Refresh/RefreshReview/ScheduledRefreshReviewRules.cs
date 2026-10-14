namespace Acme.Workforce.Assets.Refresh.RefreshReview;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled refresh review must pass. Check returns its problems, or none.</summary>
public interface IScheduledRefreshReviewRule
{
    List<string> Check(ScheduledRefreshReview item);
}

public sealed record ScheduledRefreshReviewSummaryLengthRule(int MaxLength) : IScheduledRefreshReviewRule
{
    public List<string> Check(ScheduledRefreshReview item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRefreshReviewTenureMonthsRangeRule(int Min, int Max) : IScheduledRefreshReviewRule
{
    public List<string> Check(ScheduledRefreshReview item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRefreshReviewRequiredSummaryRule(string Message) : IScheduledRefreshReviewRule
{
    public List<string> Check(ScheduledRefreshReview item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRefreshReviewAllowedCodeRule(List<string> Allowed) : IScheduledRefreshReviewRule
{
    public List<string> Check(ScheduledRefreshReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRefreshReviewRevisionRangeRule(int Min, int Max) : IScheduledRefreshReviewRule
{
    public List<string> Check(ScheduledRefreshReview item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled refresh review.</summary>
public sealed record ScheduledRefreshReviewValidator(List<IScheduledRefreshReviewRule> Rules)
{
    public List<string> Validate(ScheduledRefreshReview item)
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

    public void RequireValid(ScheduledRefreshReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledRefreshReview> items)
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

/// <summary>Functions of scheduled refresh review rules.</summary>
public static class ScheduledRefreshReviewRules
{
    public static ScheduledRefreshReviewValidator DefaultScheduledRefreshReviewValidator()
    {
        return new ScheduledRefreshReviewValidator(
            new List<IScheduledRefreshReviewRule>
            {
                new ScheduledRefreshReviewSummaryLengthRule(16),
                new ScheduledRefreshReviewTenureMonthsRangeRule(80, 255),
                new ScheduledRefreshReviewRequiredSummaryRule("scheduled refresh review needs an entry"),
                new ScheduledRefreshReviewAllowedCodeRule(new List<string>()),
                new ScheduledRefreshReviewRevisionRangeRule(32, 86400),
            });
    }
}
