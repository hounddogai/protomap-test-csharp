namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment assignment must pass. Check returns its problems, or none.</summary>
public interface ISentimentAssignmentRule
{
    List<string> Check(SentimentAssignment item);
}

public sealed record SentimentAssignmentBatchSizeRangeRule(int Min, int Max) : ISentimentAssignmentRule
{
    public List<string> Check(SentimentAssignment item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SentimentAssignmentTitleLengthRule(int MaxLength) : ISentimentAssignmentRule
{
    public List<string> Check(SentimentAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAssignmentCodePrefixRule(string Prefix) : ISentimentAssignmentRule
{
    public List<string> Check(SentimentAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentAssignmentAllowedCodeRule(List<string> Allowed) : ISentimentAssignmentRule
{
    public List<string> Check(SentimentAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment assignment.</summary>
public sealed record SentimentAssignmentValidator(List<ISentimentAssignmentRule> Rules)
{
    public List<string> Validate(SentimentAssignment item)
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

    public void RequireValid(SentimentAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentAssignment> items)
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

/// <summary>Functions of sentiment assignment rules.</summary>
public static class SentimentAssignmentRules
{
    public static SentimentAssignmentValidator DefaultSentimentAssignmentValidator()
    {
        return new SentimentAssignmentValidator(
            new List<ISentimentAssignmentRule>
            {
                new SentimentAssignmentBatchSizeRangeRule(32, 255),
                new SentimentAssignmentTitleLengthRule(64),
                new SentimentAssignmentCodePrefixRule("REF"),
                new SentimentAssignmentAllowedCodeRule(new List<string>()),
            });
    }
}
