namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered sentiment target must pass. Check returns its problems, or none.</summary>
public interface ITieredSentimentTargetRule
{
    List<string> Check(TieredSentimentTarget item);
}

public sealed record TieredSentimentTargetAllowedLabelRule(List<string> Allowed) : ITieredSentimentTargetRule
{
    public List<string> Check(TieredSentimentTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentTargetLabelLengthRule(int MaxLength) : ITieredSentimentTargetRule
{
    public List<string> Check(TieredSentimentTarget item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredSentimentTargetCapacityRangeRule(int Min, int Max) : ITieredSentimentTargetRule
{
    public List<string> Check(TieredSentimentTarget item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredSentimentTargetCodePrefixRule(string Prefix) : ITieredSentimentTargetRule
{
    public List<string> Check(TieredSentimentTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered sentiment target.</summary>
public sealed record TieredSentimentTargetValidator(List<ITieredSentimentTargetRule> Rules)
{
    public List<string> Validate(TieredSentimentTarget item)
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

    public void RequireValid(TieredSentimentTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredSentimentTarget> items)
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

/// <summary>Functions of tiered sentiment target rules.</summary>
public static class TieredSentimentTargetRules
{
    public static TieredSentimentTargetValidator DefaultTieredSentimentTargetValidator()
    {
        return new TieredSentimentTargetValidator(
            new List<ITieredSentimentTargetRule>
            {
                new TieredSentimentTargetAllowedLabelRule(new List<string> { "strict" }),
                new TieredSentimentTargetLabelLengthRule(255),
                new TieredSentimentTargetCapacityRangeRule(255, 255),
                new TieredSentimentTargetCodePrefixRule("REF"),
            });
    }
}
