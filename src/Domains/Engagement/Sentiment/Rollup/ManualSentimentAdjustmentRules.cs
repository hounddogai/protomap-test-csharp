namespace Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>A rule a manual sentiment adjustment must pass. Check returns its problems, or none.</summary>
public interface IManualSentimentAdjustmentRule
{
    List<string> Check(ManualSentimentAdjustment item);
}

public sealed record ManualSentimentAdjustmentMinimumRangeRule(double Min, double Max) : IManualSentimentAdjustmentRule
{
    public List<string> Check(ManualSentimentAdjustment item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSentimentAdjustmentNameLengthRule(int MaxLength) : IManualSentimentAdjustmentRule
{
    public List<string> Check(ManualSentimentAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSentimentAdjustmentCodeLengthRule(int MaxLength) : IManualSentimentAdjustmentRule
{
    public List<string> Check(ManualSentimentAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual sentiment adjustment.</summary>
public sealed record ManualSentimentAdjustmentValidator(List<IManualSentimentAdjustmentRule> Rules)
{
    public List<string> Validate(ManualSentimentAdjustment item)
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

    public void RequireValid(ManualSentimentAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualSentimentAdjustment> items)
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

/// <summary>Functions of manual sentiment adjustment rules.</summary>
public static class ManualSentimentAdjustmentRules
{
    public static ManualSentimentAdjustmentValidator DefaultManualSentimentAdjustmentValidator()
    {
        return new ManualSentimentAdjustmentValidator(
            new List<IManualSentimentAdjustmentRule>
            {
                new ManualSentimentAdjustmentMinimumRangeRule(7.3, 2979.4),
                new ManualSentimentAdjustmentNameLengthRule(120),
                new ManualSentimentAdjustmentCodeLengthRule(255),
            });
    }
}
