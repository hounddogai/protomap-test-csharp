namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary sentiment bundle must pass. Check returns its problems, or none.</summary>
public interface IPrimarySentimentBundleRule
{
    List<string> Check(PrimarySentimentBundle item);
}

public sealed record PrimarySentimentBundleAllowedTitleRule(List<string> Allowed) : IPrimarySentimentBundleRule
{
    public List<string> Check(PrimarySentimentBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySentimentBundleActiveGuardRule(int Limit) : IPrimarySentimentBundleRule
{
    public List<string> Check(PrimarySentimentBundle item)
    {
        if (item.Active && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySentimentBundleCodeLengthRule(int MaxLength) : IPrimarySentimentBundleRule
{
    public List<string> Check(PrimarySentimentBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySentimentBundleCodePrefixRule(string Prefix) : IPrimarySentimentBundleRule
{
    public List<string> Check(PrimarySentimentBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySentimentBundleCreatedAtRangeRule(int Min, int Max) : IPrimarySentimentBundleRule
{
    public List<string> Check(PrimarySentimentBundle item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary sentiment bundle.</summary>
public sealed record PrimarySentimentBundleValidator(List<IPrimarySentimentBundleRule> Rules)
{
    public List<string> Validate(PrimarySentimentBundle item)
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

    public void RequireValid(PrimarySentimentBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimarySentimentBundle> items)
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

/// <summary>Functions of primary sentiment bundle rules.</summary>
public static class PrimarySentimentBundleRules
{
    public static PrimarySentimentBundleValidator DefaultPrimarySentimentBundleValidator()
    {
        return new PrimarySentimentBundleValidator(
            new List<IPrimarySentimentBundleRule>
            {
                new PrimarySentimentBundleAllowedTitleRule(new List<string> { "scheduled", "rolling" }),
                new PrimarySentimentBundleActiveGuardRule(86400),
                new PrimarySentimentBundleCodeLengthRule(3600),
                new PrimarySentimentBundleCodePrefixRule("ACM"),
                new PrimarySentimentBundleCreatedAtRangeRule(64, 80),
            });
    }
}
