namespace Acme.Workforce.Engagement.Sentiment.SentimentSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered sentiment summary must pass. Check returns its problems, or none.</summary>
public interface ITieredSentimentSummaryRule
{
    List<string> Check(TieredSentimentSummary item);
}

public sealed record TieredSentimentSummaryGraceDaysRangeRule(int Min, int Max) : ITieredSentimentSummaryRule
{
    public List<string> Check(TieredSentimentSummary item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentSummaryRequiredNameRule(string Message) : ITieredSentimentSummaryRule
{
    public List<string> Check(TieredSentimentSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentSummaryRequiredEmployeeIdRule(string Message) : ITieredSentimentSummaryRule
{
    public List<string> Check(TieredSentimentSummary item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentSummaryNameLengthRule(int MaxLength) : ITieredSentimentSummaryRule
{
    public List<string> Check(TieredSentimentSummary item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentSummaryCapacityRangeRule(int Min, int Max) : ITieredSentimentSummaryRule
{
    public List<string> Check(TieredSentimentSummary item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredSentimentSummaryAllowedNameRule(List<string> Allowed) : ITieredSentimentSummaryRule
{
    public List<string> Check(TieredSentimentSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered sentiment summary.</summary>
public sealed record TieredSentimentSummaryValidator(List<ITieredSentimentSummaryRule> Rules)
{
    public List<string> Validate(TieredSentimentSummary item)
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

    public void RequireValid(TieredSentimentSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSentimentSummary> items)
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

/// <summary>Functions of tiered sentiment summary rules.</summary>
public static class TieredSentimentSummaryRules
{
    public static TieredSentimentSummaryValidator DefaultTieredSentimentSummaryValidator()
    {
        return new TieredSentimentSummaryValidator(
            new List<ITieredSentimentSummaryRule>
            {
                new TieredSentimentSummaryGraceDaysRangeRule(32, 80),
                new TieredSentimentSummaryRequiredNameRule("tiered sentiment summary needs an entry"),
                new TieredSentimentSummaryRequiredEmployeeIdRule("tiered sentiment summary needs a value"),
                new TieredSentimentSummaryNameLengthRule(32),
                new TieredSentimentSummaryCapacityRangeRule(120, 86400),
                new TieredSentimentSummaryAllowedNameRule(new List<string> { "legacy", "global", "global" }),
            });
    }
}
