namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment snapshot must pass. Check returns its problems, or none.</summary>
public interface ISentimentSnapshotRule
{
    List<string> Check(SentimentSnapshot item);
}

public sealed record SentimentSnapshotTitleLengthRule(int MaxLength) : ISentimentSnapshotRule
{
    public List<string> Check(SentimentSnapshot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentSnapshotSegmentLengthRule(int MaxLength) : ISentimentSnapshotRule
{
    public List<string> Check(SentimentSnapshot item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SentimentSnapshotAllowedLabelRule(List<string> Allowed) : ISentimentSnapshotRule
{
    public List<string> Check(SentimentSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a sentiment snapshot.</summary>
public sealed record SentimentSnapshotValidator(List<ISentimentSnapshotRule> Rules)
{
    public List<string> Validate(SentimentSnapshot item)
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

    public void RequireValid(SentimentSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentSnapshot> items)
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

/// <summary>Functions of sentiment snapshot rules.</summary>
public static class SentimentSnapshotRules
{
    public static SentimentSnapshotValidator DefaultSentimentSnapshotValidator()
    {
        return new SentimentSnapshotValidator(
            new List<ISentimentSnapshotRule>
            {
                new SentimentSnapshotTitleLengthRule(32),
                new SentimentSnapshotSegmentLengthRule(16),
                new SentimentSnapshotAllowedLabelRule(new List<string> { "regional", "adaptive", "pending" }),
            });
    }
}
