namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>A rule a sentiment exception must pass. Check returns its problems, or none.</summary>
public interface ISentimentExceptionRule
{
    List<string> Check(SentimentException item);
}

public sealed record SentimentExceptionRequiredLabelRule(string Message) : ISentimentExceptionRule
{
    public List<string> Check(SentimentException item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentExceptionCodeLengthRule(int MaxLength) : ISentimentExceptionRule
{
    public List<string> Check(SentimentException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentExceptionMaximumRangeRule(double Min, double Max) : ISentimentExceptionRule
{
    public List<string> Check(SentimentException item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentExceptionLabelLengthRule(int MaxLength) : ISentimentExceptionRule
{
    public List<string> Check(SentimentException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentExceptionRequiredGradeRule(string Message) : ISentimentExceptionRule
{
    public List<string> Check(SentimentException item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentExceptionRequiredCodeRule(string Message) : ISentimentExceptionRule
{
    public List<string> Check(SentimentException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment exception.</summary>
public sealed record SentimentExceptionValidator(List<ISentimentExceptionRule> Rules)
{
    public List<string> Validate(SentimentException item)
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

    public void RequireValid(SentimentException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentException> items)
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

/// <summary>Functions of sentiment exception rules.</summary>
public static class SentimentExceptionRules
{
    public static SentimentExceptionValidator DefaultSentimentExceptionValidator()
    {
        return new SentimentExceptionValidator(
            new List<ISentimentExceptionRule>
            {
                new SentimentExceptionRequiredLabelRule("sentiment exception needs a value"),
                new SentimentExceptionCodeLengthRule(120),
                new SentimentExceptionMaximumRangeRule(8.8, 4357.4),
                new SentimentExceptionLabelLengthRule(32),
                new SentimentExceptionRequiredGradeRule("sentiment exception needs a value"),
                new SentimentExceptionRequiredCodeRule("sentiment exception needs this field"),
            });
    }
}
