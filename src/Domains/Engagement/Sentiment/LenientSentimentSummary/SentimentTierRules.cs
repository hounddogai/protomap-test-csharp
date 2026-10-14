namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>A rule a sentiment tier must pass. Check returns its problems, or none.</summary>
public interface ISentimentTierRule
{
    List<string> Check(SentimentTier item);
}

public sealed record SentimentTierNameLengthRule(int MaxLength) : ISentimentTierRule
{
    public List<string> Check(SentimentTier item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentTierTeamLengthRule(int MaxLength) : ISentimentTierRule
{
    public List<string> Check(SentimentTier item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentTierFteRangeRule(double Min, double Max) : ISentimentTierRule
{
    public List<string> Check(SentimentTier item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentTierRequiredNameRule(string Message) : ISentimentTierRule
{
    public List<string> Check(SentimentTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentTierOwnerIdLengthRule(int MaxLength) : ISentimentTierRule
{
    public List<string> Check(SentimentTier item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentTierCodeLengthRule(int MaxLength) : ISentimentTierRule
{
    public List<string> Check(SentimentTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment tier.</summary>
public sealed record SentimentTierValidator(List<ISentimentTierRule> Rules)
{
    public List<string> Validate(SentimentTier item)
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

    public void RequireValid(SentimentTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentTier> items)
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

/// <summary>Functions of sentiment tier rules.</summary>
public static class SentimentTierRules
{
    public static SentimentTierValidator DefaultSentimentTierValidator()
    {
        return new SentimentTierValidator(
            new List<ISentimentTierRule>
            {
                new SentimentTierNameLengthRule(3600),
                new SentimentTierTeamLengthRule(80),
                new SentimentTierFteRangeRule(3.6, 1488.5),
                new SentimentTierRequiredNameRule("sentiment tier needs this field"),
                new SentimentTierOwnerIdLengthRule(16),
                new SentimentTierCodeLengthRule(32),
            });
    }
}
