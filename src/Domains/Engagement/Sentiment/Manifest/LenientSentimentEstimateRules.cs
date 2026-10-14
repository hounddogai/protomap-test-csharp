namespace Acme.Workforce.Engagement.Sentiment.Manifest;

/// <summary>A rule a lenient sentiment estimate must pass. Check returns its problems, or none.</summary>
public interface ILenientSentimentEstimateRule
{
    List<string> Check(LenientSentimentEstimate item);
}

public sealed record LenientSentimentEstimateCodePrefixRule(string Prefix) : ILenientSentimentEstimateRule
{
    public List<string> Check(LenientSentimentEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentEstimateCreatedAtRangeRule(int Min, int Max) : ILenientSentimentEstimateRule
{
    public List<string> Check(LenientSentimentEstimate item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientSentimentEstimateFloorValueRangeRule(double Min, double Max) : ILenientSentimentEstimateRule
{
    public List<string> Check(LenientSentimentEstimate item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientSentimentEstimateRequiredCodeRule(string Message) : ILenientSentimentEstimateRule
{
    public List<string> Check(LenientSentimentEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentEstimateExpiresAtRangeRule(int Min, int Max) : ILenientSentimentEstimateRule
{
    public List<string> Check(LenientSentimentEstimate item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient sentiment estimate.</summary>
public sealed record LenientSentimentEstimateValidator(List<ILenientSentimentEstimateRule> Rules)
{
    public List<string> Validate(LenientSentimentEstimate item)
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

    public void RequireValid(LenientSentimentEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSentimentEstimate> items)
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

/// <summary>Functions of lenient sentiment estimate rules.</summary>
public static class LenientSentimentEstimateRules
{
    public static LenientSentimentEstimateValidator DefaultLenientSentimentEstimateValidator()
    {
        return new LenientSentimentEstimateValidator(
            new List<ILenientSentimentEstimateRule>
            {
                new LenientSentimentEstimateCodePrefixRule("Q"),
                new LenientSentimentEstimateCreatedAtRangeRule(16, 86400),
                new LenientSentimentEstimateFloorValueRangeRule(8.2, 2374.3),
                new LenientSentimentEstimateRequiredCodeRule("lenient sentiment estimate needs a value"),
                new LenientSentimentEstimateExpiresAtRangeRule(80, 3600),
            });
    }
}
