namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment forecast must pass. Check returns its problems, or none.</summary>
public interface ISentimentForecastRule
{
    List<string> Check(SentimentForecast item);
}

public sealed record SentimentForecastRequiredLabelRule(string Message) : ISentimentForecastRule
{
    public List<string> Check(SentimentForecast item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentForecastAllowedCodeRule(List<string> Allowed) : ISentimentForecastRule
{
    public List<string> Check(SentimentForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record SentimentForecastCodePrefixRule(string Prefix) : ISentimentForecastRule
{
    public List<string> Check(SentimentForecast item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentForecastCodeLengthRule(int MaxLength) : ISentimentForecastRule
{
    public List<string> Check(SentimentForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment forecast.</summary>
public sealed record SentimentForecastValidator(List<ISentimentForecastRule> Rules)
{
    public List<string> Validate(SentimentForecast item)
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

    public void RequireValid(SentimentForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentForecast> items)
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

/// <summary>Functions of sentiment forecast rules.</summary>
public static class SentimentForecastRules
{
    public static SentimentForecastValidator DefaultSentimentForecastValidator()
    {
        return new SentimentForecastValidator(
            new List<ISentimentForecastRule>
            {
                new SentimentForecastRequiredLabelRule("sentiment forecast needs a value"),
                new SentimentForecastAllowedCodeRule(new List<string> { "weighted", "rolling", "lenient" }),
                new SentimentForecastCodePrefixRule("REF"),
                new SentimentForecastCodeLengthRule(255),
            });
    }
}
