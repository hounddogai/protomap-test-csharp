namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict engagement snapshot must pass. Check returns its problems, or none.</summary>
public interface IStrictEngagementSnapshotRule
{
    List<string> Check(StrictEngagementSnapshot item);
}

public sealed record StrictEngagementSnapshotAllowedTitleRule(List<string> Allowed) : IStrictEngagementSnapshotRule
{
    public List<string> Check(StrictEngagementSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

public sealed record StrictEngagementSnapshotRequiredNotesRule(string Message) : IStrictEngagementSnapshotRule
{
    public List<string> Check(StrictEngagementSnapshot item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictEngagementSnapshotScoreRangeRule(double Min, double Max) : IStrictEngagementSnapshotRule
{
    public List<string> Check(StrictEngagementSnapshot item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictEngagementSnapshotNotesLengthRule(int MaxLength) : IStrictEngagementSnapshotRule
{
    public List<string> Check(StrictEngagementSnapshot item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict engagement snapshot.</summary>
public sealed record StrictEngagementSnapshotValidator(List<IStrictEngagementSnapshotRule> Rules)
{
    public List<string> Validate(StrictEngagementSnapshot item)
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

    public void RequireValid(StrictEngagementSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictEngagementSnapshot> items)
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

/// <summary>Functions of strict engagement snapshot rules.</summary>
public static class StrictEngagementSnapshotRules
{
    public static StrictEngagementSnapshotValidator DefaultStrictEngagementSnapshotValidator()
    {
        return new StrictEngagementSnapshotValidator(
            new List<IStrictEngagementSnapshotRule>
            {
                new StrictEngagementSnapshotAllowedTitleRule(new List<string>()),
                new StrictEngagementSnapshotRequiredNotesRule("strict engagement snapshot needs an entry"),
                new StrictEngagementSnapshotScoreRangeRule(5.1, 1250.2),
                new StrictEngagementSnapshotNotesLengthRule(86400),
            });
    }
}
