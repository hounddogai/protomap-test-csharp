namespace Acme.Workforce.Engagement.Sentiment.Milestone;

/// <summary>A rule a sentiment ledger must pass. Check returns its problems, or none.</summary>
public interface ISentimentLedgerRule
{
    List<string> Check(SentimentLedger item);
}

public sealed record SentimentLedgerCodePrefixRule(string Prefix) : ISentimentLedgerRule
{
    public List<string> Check(SentimentLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentLedgerCostRangeRule(double Min, double Max) : ISentimentLedgerRule
{
    public List<string> Check(SentimentLedger item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentLedgerRequiredCodeRule(string Message) : ISentimentLedgerRule
{
    public List<string> Check(SentimentLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentLedgerRequiredLabelRule(string Message) : ISentimentLedgerRule
{
    public List<string> Check(SentimentLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment ledger.</summary>
public sealed record SentimentLedgerValidator(List<ISentimentLedgerRule> Rules)
{
    public List<string> Validate(SentimentLedger item)
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

    public void RequireValid(SentimentLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentLedger> items)
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

/// <summary>Functions of sentiment ledger rules.</summary>
public static class SentimentLedgerRules
{
    public static SentimentLedgerValidator DefaultSentimentLedgerValidator()
    {
        return new SentimentLedgerValidator(
            new List<ISentimentLedgerRule>
            {
                new SentimentLedgerCodePrefixRule("ACM"),
                new SentimentLedgerCostRangeRule(5.8, 1762.4),
                new SentimentLedgerRequiredCodeRule("sentiment ledger needs this field"),
                new SentimentLedgerRequiredLabelRule("sentiment ledger needs an entry"),
            });
    }
}
