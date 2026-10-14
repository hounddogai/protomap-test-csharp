namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient sentiment exception must pass. Check returns its problems, or none.</summary>
public interface ILenientSentimentExceptionRule
{
    List<string> Check(LenientSentimentException item);
}

public sealed record LenientSentimentExceptionJobCodeLengthRule(int MaxLength) : ILenientSentimentExceptionRule
{
    public List<string> Check(LenientSentimentException item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentExceptionTagsLimitRule(int MaxCount, List<string> Banned) : ILenientSentimentExceptionRule
{
    public List<string> Check(LenientSentimentException item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record LenientSentimentExceptionCreatedAtRangeRule(int Min, int Max) : ILenientSentimentExceptionRule
{
    public List<string> Check(LenientSentimentException item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSentimentExceptionAllowedJobCodeRule(List<string> Allowed) : ILenientSentimentExceptionRule
{
    public List<string> Check(LenientSentimentException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient sentiment exception.</summary>
public sealed record LenientSentimentExceptionValidator(List<ILenientSentimentExceptionRule> Rules)
{
    public List<string> Validate(LenientSentimentException item)
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

    public void RequireValid(LenientSentimentException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSentimentException> items)
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

/// <summary>Functions of lenient sentiment exception rules.</summary>
public static class LenientSentimentExceptionRules
{
    public static LenientSentimentExceptionValidator DefaultLenientSentimentExceptionValidator()
    {
        return new LenientSentimentExceptionValidator(
            new List<ILenientSentimentExceptionRule>
            {
                new LenientSentimentExceptionJobCodeLengthRule(86400),
                new LenientSentimentExceptionTagsLimitRule(16, new List<string>()),
                new LenientSentimentExceptionCreatedAtRangeRule(16, 16),
                new LenientSentimentExceptionAllowedJobCodeRule(new List<string> { "fallback", "regional" }),
            });
    }
}
