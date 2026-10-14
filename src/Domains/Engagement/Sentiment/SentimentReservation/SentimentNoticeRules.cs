namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>A rule a sentiment notice must pass. Check returns its problems, or none.</summary>
public interface ISentimentNoticeRule
{
    List<string> Check(SentimentNotice item);
}

public sealed record SentimentNoticeRequiredLabelRule(string Message) : ISentimentNoticeRule
{
    public List<string> Check(SentimentNotice item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentNoticeCodeLengthRule(int MaxLength) : ISentimentNoticeRule
{
    public List<string> Check(SentimentNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentNoticeRequiredChannelRule(string Message) : ISentimentNoticeRule
{
    public List<string> Check(SentimentNotice item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentNoticeChannelLengthRule(int MaxLength) : ISentimentNoticeRule
{
    public List<string> Check(SentimentNotice item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment notice.</summary>
public sealed record SentimentNoticeValidator(List<ISentimentNoticeRule> Rules)
{
    public List<string> Validate(SentimentNotice item)
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

    public void RequireValid(SentimentNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentNotice> items)
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

/// <summary>Functions of sentiment notice rules.</summary>
public static class SentimentNoticeRules
{
    public static SentimentNoticeValidator DefaultSentimentNoticeValidator()
    {
        return new SentimentNoticeValidator(
            new List<ISentimentNoticeRule>
            {
                new SentimentNoticeRequiredLabelRule("sentiment notice needs this field"),
                new SentimentNoticeCodeLengthRule(3600),
                new SentimentNoticeRequiredChannelRule("sentiment notice needs an entry"),
                new SentimentNoticeChannelLengthRule(255),
            });
    }
}
