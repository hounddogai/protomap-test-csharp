namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled sentiment target must pass. Check returns its problems, or none.</summary>
public interface IScheduledSentimentTargetRule
{
    List<string> Check(ScheduledSentimentTarget item);
}

public sealed record ScheduledSentimentTargetAllowedSourceRule(List<string> Allowed) : IScheduledSentimentTargetRule
{
    public List<string> Check(ScheduledSentimentTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is expired" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSentimentTargetCodeLengthRule(int MaxLength) : IScheduledSentimentTargetRule
{
    public List<string> Check(ScheduledSentimentTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSentimentTargetGraceDaysRangeRule(int Min, int Max) : IScheduledSentimentTargetRule
{
    public List<string> Check(ScheduledSentimentTarget item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSentimentTargetLimitRangeRule(int Min, int Max) : IScheduledSentimentTargetRule
{
    public List<string> Check(ScheduledSentimentTarget item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledSentimentTargetRequiredNameRule(string Message) : IScheduledSentimentTargetRule
{
    public List<string> Check(ScheduledSentimentTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled sentiment target.</summary>
public sealed record ScheduledSentimentTargetValidator(List<IScheduledSentimentTargetRule> Rules)
{
    public List<string> Validate(ScheduledSentimentTarget item)
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

    public void RequireValid(ScheduledSentimentTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledSentimentTarget> items)
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

/// <summary>Functions of scheduled sentiment target rules.</summary>
public static class ScheduledSentimentTargetRules
{
    public static ScheduledSentimentTargetValidator DefaultScheduledSentimentTargetValidator()
    {
        return new ScheduledSentimentTargetValidator(
            new List<IScheduledSentimentTargetRule>
            {
                new ScheduledSentimentTargetAllowedSourceRule(new List<string> { "lenient" }),
                new ScheduledSentimentTargetCodeLengthRule(1000),
                new ScheduledSentimentTargetGraceDaysRangeRule(32, 32),
                new ScheduledSentimentTargetLimitRangeRule(255, 1000),
                new ScheduledSentimentTargetRequiredNameRule("scheduled sentiment target needs this field"),
            });
    }
}
