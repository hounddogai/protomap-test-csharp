namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment profile must pass. Check returns its problems, or none.</summary>
public interface ISentimentProfileRule
{
    List<string> Check(SentimentProfile item);
}

public sealed record SentimentProfileAllowedCountryRule(List<string> Allowed) : ISentimentProfileRule
{
    public List<string> Check(SentimentProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record SentimentProfileSourceLengthRule(int MaxLength) : ISentimentProfileRule
{
    public List<string> Check(SentimentProfile item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentProfileLeaveDaysRangeRule(int Min, int Max) : ISentimentProfileRule
{
    public List<string> Check(SentimentProfile item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SentimentProfileRequiredNameRule(string Message) : ISentimentProfileRule
{
    public List<string> Check(SentimentProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment profile.</summary>
public sealed record SentimentProfileValidator(List<ISentimentProfileRule> Rules)
{
    public List<string> Validate(SentimentProfile item)
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

    public void RequireValid(SentimentProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentProfile> items)
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

/// <summary>Functions of sentiment profile rules.</summary>
public static class SentimentProfileRules
{
    public static SentimentProfileValidator DefaultSentimentProfileValidator()
    {
        return new SentimentProfileValidator(
            new List<ISentimentProfileRule>
            {
                new SentimentProfileAllowedCountryRule(new List<string>()),
                new SentimentProfileSourceLengthRule(16),
                new SentimentProfileLeaveDaysRangeRule(16, 64),
                new SentimentProfileRequiredNameRule("sentiment profile needs this field"),
            });
    }
}
