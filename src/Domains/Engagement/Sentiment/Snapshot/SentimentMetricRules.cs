namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>A rule a sentiment metric must pass. Check returns its problems, or none.</summary>
public interface ISentimentMetricRule
{
    List<string> Check(SentimentMetric item);
}

public sealed record SentimentMetricCodeLengthRule(int MaxLength) : ISentimentMetricRule
{
    public List<string> Check(SentimentMetric item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentMetricMaximumRangeRule(double Min, double Max) : ISentimentMetricRule
{
    public List<string> Check(SentimentMetric item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentMetricRequiredLocationRule(string Message) : ISentimentMetricRule
{
    public List<string> Check(SentimentMetric item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentMetricCodePrefixRule(string Prefix) : ISentimentMetricRule
{
    public List<string> Check(SentimentMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentMetricRequiredCodeRule(string Message) : ISentimentMetricRule
{
    public List<string> Check(SentimentMetric item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment metric.</summary>
public sealed record SentimentMetricValidator(List<ISentimentMetricRule> Rules)
{
    public List<string> Validate(SentimentMetric item)
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

    public void RequireValid(SentimentMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentMetric> items)
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

/// <summary>Functions of sentiment metric rules.</summary>
public static class SentimentMetricRules
{
    public static SentimentMetricValidator DefaultSentimentMetricValidator()
    {
        return new SentimentMetricValidator(
            new List<ISentimentMetricRule>
            {
                new SentimentMetricCodeLengthRule(120),
                new SentimentMetricMaximumRangeRule(1.3, 1796.6),
                new SentimentMetricRequiredLocationRule("sentiment metric needs this field"),
                new SentimentMetricCodePrefixRule("REF"),
                new SentimentMetricRequiredCodeRule("sentiment metric needs a value"),
            });
    }
}
