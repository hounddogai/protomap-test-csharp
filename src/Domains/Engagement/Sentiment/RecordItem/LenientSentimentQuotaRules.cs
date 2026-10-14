namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

/// <summary>A rule a lenient sentiment quota must pass. Check returns its problems, or none.</summary>
public interface ILenientSentimentQuotaRule
{
    List<string> Check(LenientSentimentQuota item);
}

public sealed record LenientSentimentQuotaMaximumRangeRule(double Min, double Max) : ILenientSentimentQuotaRule
{
    public List<string> Check(LenientSentimentQuota item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentQuotaTitleLengthRule(int MaxLength) : ILenientSentimentQuotaRule
{
    public List<string> Check(LenientSentimentQuota item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentQuotaEndsAtRangeRule(int Min, int Max) : ILenientSentimentQuotaRule
{
    public List<string> Check(LenientSentimentQuota item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient sentiment quota.</summary>
public sealed record LenientSentimentQuotaValidator(List<ILenientSentimentQuotaRule> Rules)
{
    public List<string> Validate(LenientSentimentQuota item)
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

    public void RequireValid(LenientSentimentQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSentimentQuota> items)
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

/// <summary>Functions of lenient sentiment quota rules.</summary>
public static class LenientSentimentQuotaRules
{
    public static LenientSentimentQuotaValidator DefaultLenientSentimentQuotaValidator()
    {
        return new LenientSentimentQuotaValidator(
            new List<ILenientSentimentQuotaRule>
            {
                new LenientSentimentQuotaMaximumRangeRule(6.6, 1839.9),
                new LenientSentimentQuotaTitleLengthRule(255),
                new LenientSentimentQuotaEndsAtRangeRule(1000, 3600),
            });
    }
}
