namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered sentiment adjustment must pass. Check returns its problems, or none.</summary>
public interface ITieredSentimentAdjustmentRule
{
    List<string> Check(TieredSentimentAdjustment item);
}

public sealed record TieredSentimentAdjustmentRetriesRangeRule(int Min, int Max) : ITieredSentimentAdjustmentRule
{
    public List<string> Check(TieredSentimentAdjustment item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentAdjustmentCodeLengthRule(int MaxLength) : ITieredSentimentAdjustmentRule
{
    public List<string> Check(TieredSentimentAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentAdjustmentRequiredCodeRule(string Message) : ITieredSentimentAdjustmentRule
{
    public List<string> Check(TieredSentimentAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentAdjustmentAllowedLabelRule(List<string> Allowed) : ITieredSentimentAdjustmentRule
{
    public List<string> Check(TieredSentimentAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentAdjustmentCreatedAtRangeRule(int Min, int Max) : ITieredSentimentAdjustmentRule
{
    public List<string> Check(TieredSentimentAdjustment item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentAdjustmentScoreRangeRule(double Min, double Max) : ITieredSentimentAdjustmentRule
{
    public List<string> Check(TieredSentimentAdjustment item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tiered sentiment adjustment.</summary>
public sealed record TieredSentimentAdjustmentValidator(List<ITieredSentimentAdjustmentRule> Rules)
{
    public List<string> Validate(TieredSentimentAdjustment item)
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

    public void RequireValid(TieredSentimentAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSentimentAdjustment> items)
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

/// <summary>Functions of tiered sentiment adjustment rules.</summary>
public static class TieredSentimentAdjustmentRules
{
    public static TieredSentimentAdjustmentValidator DefaultTieredSentimentAdjustmentValidator()
    {
        return new TieredSentimentAdjustmentValidator(
            new List<ITieredSentimentAdjustmentRule>
            {
                new TieredSentimentAdjustmentRetriesRangeRule(255, 86400),
                new TieredSentimentAdjustmentCodeLengthRule(64),
                new TieredSentimentAdjustmentRequiredCodeRule("tiered sentiment adjustment needs this field"),
                new TieredSentimentAdjustmentAllowedLabelRule(new List<string> { "primary", "regional" }),
                new TieredSentimentAdjustmentCreatedAtRangeRule(120, 3600),
                new TieredSentimentAdjustmentScoreRangeRule(0.0, 914.8),
            });
    }
}
