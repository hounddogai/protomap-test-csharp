namespace Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>A rule a pending sentiment variance must pass. Check returns its problems, or none.</summary>
public interface IPendingSentimentVarianceRule
{
    List<string> Check(PendingSentimentVariance item);
}

public sealed record PendingSentimentVarianceCodePrefixRule(string Prefix) : IPendingSentimentVarianceRule
{
    public List<string> Check(PendingSentimentVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentVarianceVisibleGuardRule(double Limit) : IPendingSentimentVarianceRule
{
    public List<string> Check(PendingSentimentVariance item)
    {
        if (item.Visible && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentVarianceCodeLengthRule(int MaxLength) : IPendingSentimentVarianceRule
{
    public List<string> Check(PendingSentimentVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSentimentVarianceRequiredNameRule(string Message) : IPendingSentimentVarianceRule
{
    public List<string> Check(PendingSentimentVariance item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending sentiment variance.</summary>
public sealed record PendingSentimentVarianceValidator(List<IPendingSentimentVarianceRule> Rules)
{
    public List<string> Validate(PendingSentimentVariance item)
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

    public void RequireValid(PendingSentimentVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSentimentVariance> items)
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

/// <summary>Functions of pending sentiment variance rules.</summary>
public static class PendingSentimentVarianceRules
{
    public static PendingSentimentVarianceValidator DefaultPendingSentimentVarianceValidator()
    {
        return new PendingSentimentVarianceValidator(
            new List<IPendingSentimentVarianceRule>
            {
                new PendingSentimentVarianceCodePrefixRule("OPS"),
                new PendingSentimentVarianceVisibleGuardRule(9.9),
                new PendingSentimentVarianceCodeLengthRule(1000),
                new PendingSentimentVarianceRequiredNameRule("pending sentiment variance needs an entry"),
            });
    }
}
