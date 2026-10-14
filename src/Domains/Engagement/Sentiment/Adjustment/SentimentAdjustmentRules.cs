namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment adjustment must pass. Check returns its problems, or none.</summary>
public interface ISentimentAdjustmentRule
{
    List<string> Check(SentimentAdjustment item);
}

public sealed record SentimentAdjustmentRequiredCodeRule(string Message) : ISentimentAdjustmentRule
{
    public List<string> Check(SentimentAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentAdjustmentTitleLengthRule(int MaxLength) : ISentimentAdjustmentRule
{
    public List<string> Check(SentimentAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAdjustmentCodePrefixRule(string Prefix) : ISentimentAdjustmentRule
{
    public List<string> Check(SentimentAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAdjustmentAllowedEmployeeIdRule(List<string> Allowed) : ISentimentAdjustmentRule
{
    public List<string> Check(SentimentAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAdjustmentRequiredEmployeeIdRule(string Message) : ISentimentAdjustmentRule
{
    public List<string> Check(SentimentAdjustment item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentAdjustmentBudgetRangeRule(double Min, double Max) : ISentimentAdjustmentRule
{
    public List<string> Check(SentimentAdjustment item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a sentiment adjustment.</summary>
public sealed record SentimentAdjustmentValidator(List<ISentimentAdjustmentRule> Rules)
{
    public List<string> Validate(SentimentAdjustment item)
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

    public void RequireValid(SentimentAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentAdjustment> items)
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

/// <summary>Functions of sentiment adjustment rules.</summary>
public static class SentimentAdjustmentRules
{
    public static SentimentAdjustmentValidator DefaultSentimentAdjustmentValidator()
    {
        return new SentimentAdjustmentValidator(
            new List<ISentimentAdjustmentRule>
            {
                new SentimentAdjustmentRequiredCodeRule("sentiment adjustment needs a value"),
                new SentimentAdjustmentTitleLengthRule(86400),
                new SentimentAdjustmentCodePrefixRule("ACM"),
                new SentimentAdjustmentAllowedEmployeeIdRule(new List<string> { "lenient", "strict", "strict" }),
                new SentimentAdjustmentRequiredEmployeeIdRule("sentiment adjustment needs a value"),
                new SentimentAdjustmentBudgetRangeRule(3.1, 3662.8),
            });
    }
}
