namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled recognition snapshot must pass. Check returns its problems, or none.</summary>
public interface IScheduledRecognitionSnapshotRule
{
    List<string> Check(ScheduledRecognitionSnapshot item);
}

public sealed record ScheduledRecognitionSnapshotRequiredTitleRule(string Message) : IScheduledRecognitionSnapshotRule
{
    public List<string> Check(ScheduledRecognitionSnapshot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionSnapshotTitleLengthRule(int MaxLength) : IScheduledRecognitionSnapshotRule
{
    public List<string> Check(ScheduledRecognitionSnapshot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionSnapshotAllowedCodeRule(List<string> Allowed) : IScheduledRecognitionSnapshotRule
{
    public List<string> Check(ScheduledRecognitionSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRecognitionSnapshotRegionLengthRule(int MaxLength) : IScheduledRecognitionSnapshotRule
{
    public List<string> Check(ScheduledRecognitionSnapshot item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled recognition snapshot.</summary>
public sealed record ScheduledRecognitionSnapshotValidator(List<IScheduledRecognitionSnapshotRule> Rules)
{
    public List<string> Validate(ScheduledRecognitionSnapshot item)
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

    public void RequireValid(ScheduledRecognitionSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledRecognitionSnapshot> items)
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

/// <summary>Functions of scheduled recognition snapshot rules.</summary>
public static class ScheduledRecognitionSnapshotRules
{
    public static ScheduledRecognitionSnapshotValidator DefaultScheduledRecognitionSnapshotValidator()
    {
        return new ScheduledRecognitionSnapshotValidator(
            new List<IScheduledRecognitionSnapshotRule>
            {
                new ScheduledRecognitionSnapshotRequiredTitleRule("scheduled recognition snapshot needs a value"),
                new ScheduledRecognitionSnapshotTitleLengthRule(64),
                new ScheduledRecognitionSnapshotAllowedCodeRule(new List<string>()),
                new ScheduledRecognitionSnapshotRegionLengthRule(3600),
            });
    }
}
