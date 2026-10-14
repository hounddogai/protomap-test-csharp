namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment record must pass. Check returns its problems, or none.</summary>
public interface ISentimentRecordRule
{
    List<string> Check(SentimentRecord item);
}

public sealed record SentimentRecordUpdatedAtRangeRule(int Min, int Max) : ISentimentRecordRule
{
    public List<string> Check(SentimentRecord item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SentimentRecordRequiredTimezoneRule(string Message) : ISentimentRecordRule
{
    public List<string> Check(SentimentRecord item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentRecordCodeLengthRule(int MaxLength) : ISentimentRecordRule
{
    public List<string> Check(SentimentRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentRecordAllowedLocaleRule(List<string> Allowed) : ISentimentRecordRule
{
    public List<string> Check(SentimentRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record SentimentRecordRequiredCodeRule(string Message) : ISentimentRecordRule
{
    public List<string> Check(SentimentRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment record.</summary>
public sealed record SentimentRecordValidator(List<ISentimentRecordRule> Rules)
{
    public List<string> Validate(SentimentRecord item)
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

    public void RequireValid(SentimentRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentRecord> items)
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

/// <summary>Functions of sentiment record rules.</summary>
public static class SentimentRecordRules
{
    public static SentimentRecordValidator DefaultSentimentRecordValidator()
    {
        return new SentimentRecordValidator(
            new List<ISentimentRecordRule>
            {
                new SentimentRecordUpdatedAtRangeRule(80, 80),
                new SentimentRecordRequiredTimezoneRule("sentiment record needs an entry"),
                new SentimentRecordCodeLengthRule(16),
                new SentimentRecordAllowedLocaleRule(new List<string> { "capped", "strict" }),
                new SentimentRecordRequiredCodeRule("sentiment record needs this field"),
            });
    }
}
