namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled feedback forecast must pass. Check returns its problems, or none.</summary>
public interface IScheduledFeedbackForecastRule
{
    List<string> Check(ScheduledFeedbackForecast item);
}

public sealed record ScheduledFeedbackForecastAllowedSalaryBandRule(List<string> Allowed) : IScheduledFeedbackForecastRule
{
    public List<string> Check(ScheduledFeedbackForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackForecastRequiredSalaryBandRule(string Message) : IScheduledFeedbackForecastRule
{
    public List<string> Check(ScheduledFeedbackForecast item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackForecastLocaleLengthRule(int MaxLength) : IScheduledFeedbackForecastRule
{
    public List<string> Check(ScheduledFeedbackForecast item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledFeedbackForecastRequiredCodeRule(string Message) : IScheduledFeedbackForecastRule
{
    public List<string> Check(ScheduledFeedbackForecast item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled feedback forecast.</summary>
public sealed record ScheduledFeedbackForecastValidator(List<IScheduledFeedbackForecastRule> Rules)
{
    public List<string> Validate(ScheduledFeedbackForecast item)
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

    public void RequireValid(ScheduledFeedbackForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledFeedbackForecast> items)
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

/// <summary>Functions of scheduled feedback forecast rules.</summary>
public static class ScheduledFeedbackForecastRules
{
    public static ScheduledFeedbackForecastValidator DefaultScheduledFeedbackForecastValidator()
    {
        return new ScheduledFeedbackForecastValidator(
            new List<IScheduledFeedbackForecastRule>
            {
                new ScheduledFeedbackForecastAllowedSalaryBandRule(new List<string>()),
                new ScheduledFeedbackForecastRequiredSalaryBandRule("scheduled feedback forecast needs an entry"),
                new ScheduledFeedbackForecastLocaleLengthRule(64),
                new ScheduledFeedbackForecastRequiredCodeRule("scheduled feedback forecast needs an entry"),
            });
    }
}
