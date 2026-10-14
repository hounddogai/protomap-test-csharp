namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>A rule a sentiment plan must pass. Check returns its problems, or none.</summary>
public interface ISentimentPlanRule
{
    List<string> Check(SentimentPlan item);
}

public sealed record SentimentPlanThresholdRangeRule(double Min, double Max) : ISentimentPlanRule
{
    public List<string> Check(SentimentPlan item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentPlanLabelLengthRule(int MaxLength) : ISentimentPlanRule
{
    public List<string> Check(SentimentPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentPlanCodePrefixRule(string Prefix) : ISentimentPlanRule
{
    public List<string> Check(SentimentPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentPlanCategoryLengthRule(int MaxLength) : ISentimentPlanRule
{
    public List<string> Check(SentimentPlan item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment plan.</summary>
public sealed record SentimentPlanValidator(List<ISentimentPlanRule> Rules)
{
    public List<string> Validate(SentimentPlan item)
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

    public void RequireValid(SentimentPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentPlan> items)
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

/// <summary>Functions of sentiment plan rules.</summary>
public static class SentimentPlanRules
{
    public static SentimentPlanValidator DefaultSentimentPlanValidator()
    {
        return new SentimentPlanValidator(
            new List<ISentimentPlanRule>
            {
                new SentimentPlanThresholdRangeRule(6.0, 1439.1),
                new SentimentPlanLabelLengthRule(120),
                new SentimentPlanCodePrefixRule("OPS"),
                new SentimentPlanCategoryLengthRule(32),
            });
    }
}
