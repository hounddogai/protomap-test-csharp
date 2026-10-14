namespace Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>A rule a sentiment baseline must pass. Check returns its problems, or none.</summary>
public interface ISentimentBaselineRule
{
    List<string> Check(SentimentBaseline item);
}

public sealed record SentimentBaselineRequiredDepartmentRule(string Message) : ISentimentBaselineRule
{
    public List<string> Check(SentimentBaseline item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentBaselineRequiredLabelRule(string Message) : ISentimentBaselineRule
{
    public List<string> Check(SentimentBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentBaselineCategoryLengthRule(int MaxLength) : ISentimentBaselineRule
{
    public List<string> Check(SentimentBaseline item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentBaselineRequiredCategoryRule(string Message) : ISentimentBaselineRule
{
    public List<string> Check(SentimentBaseline item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentBaselineWindowDaysRangeRule(int Min, int Max) : ISentimentBaselineRule
{
    public List<string> Check(SentimentBaseline item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a sentiment baseline.</summary>
public sealed record SentimentBaselineValidator(List<ISentimentBaselineRule> Rules)
{
    public List<string> Validate(SentimentBaseline item)
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

    public void RequireValid(SentimentBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentBaseline> items)
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

/// <summary>Functions of sentiment baseline rules.</summary>
public static class SentimentBaselineRules
{
    public static SentimentBaselineValidator DefaultSentimentBaselineValidator()
    {
        return new SentimentBaselineValidator(
            new List<ISentimentBaselineRule>
            {
                new SentimentBaselineRequiredDepartmentRule("sentiment baseline needs a value"),
                new SentimentBaselineRequiredLabelRule("sentiment baseline needs an entry"),
                new SentimentBaselineCategoryLengthRule(32),
                new SentimentBaselineRequiredCategoryRule("sentiment baseline needs an entry"),
                new SentimentBaselineWindowDaysRangeRule(255, 3600),
            });
    }
}
