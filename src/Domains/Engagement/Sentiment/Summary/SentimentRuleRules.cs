namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment rule must pass. Check returns its problems, or none.</summary>
public interface ISentimentRuleRule
{
    List<string> Check(SentimentRule item);
}

public sealed record SentimentRuleCodePrefixRule(string Prefix) : ISentimentRuleRule
{
    public List<string> Check(SentimentRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentRuleTenureMonthsRangeRule(int Min, int Max) : ISentimentRuleRule
{
    public List<string> Check(SentimentRule item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentRuleAllowedCodeRule(List<string> Allowed) : ISentimentRuleRule
{
    public List<string> Check(SentimentRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment rule.</summary>
public sealed record SentimentRuleValidator(List<ISentimentRuleRule> Rules)
{
    public List<string> Validate(SentimentRule item)
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

    public void RequireValid(SentimentRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentRule> items)
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

/// <summary>Functions of sentiment rule rules.</summary>
public static class SentimentRuleRules
{
    public static SentimentRuleValidator DefaultSentimentRuleValidator()
    {
        return new SentimentRuleValidator(
            new List<ISentimentRuleRule>
            {
                new SentimentRuleCodePrefixRule("Q"),
                new SentimentRuleTenureMonthsRangeRule(16, 255),
                new SentimentRuleAllowedCodeRule(new List<string> { "regional" }),
            });
    }
}
