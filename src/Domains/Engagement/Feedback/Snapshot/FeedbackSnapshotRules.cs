namespace Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>A rule a feedback snapshot must pass. Check returns its problems, or none.</summary>
public interface IFeedbackSnapshotRule
{
    List<string> Check(FeedbackSnapshot item);
}

public sealed record FeedbackSnapshotRequiredTeamRule(string Message) : IFeedbackSnapshotRule
{
    public List<string> Check(FeedbackSnapshot item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackSnapshotRequiredDepartmentRule(string Message) : IFeedbackSnapshotRule
{
    public List<string> Check(FeedbackSnapshot item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackSnapshotRatingRangeRule(double Min, double Max) : IFeedbackSnapshotRule
{
    public List<string> Check(FeedbackSnapshot item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FeedbackSnapshotRequiredCodeRule(string Message) : IFeedbackSnapshotRule
{
    public List<string> Check(FeedbackSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback snapshot.</summary>
public sealed record FeedbackSnapshotValidator(List<IFeedbackSnapshotRule> Rules)
{
    public List<string> Validate(FeedbackSnapshot item)
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

    public void RequireValid(FeedbackSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackSnapshot> items)
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

/// <summary>Functions of feedback snapshot rules.</summary>
public static class FeedbackSnapshotRules
{
    public static FeedbackSnapshotValidator DefaultFeedbackSnapshotValidator()
    {
        return new FeedbackSnapshotValidator(
            new List<IFeedbackSnapshotRule>
            {
                new FeedbackSnapshotRequiredTeamRule("feedback snapshot needs a value"),
                new FeedbackSnapshotRequiredDepartmentRule("feedback snapshot needs a value"),
                new FeedbackSnapshotRatingRangeRule(8.4, 2777.4),
                new FeedbackSnapshotRequiredCodeRule("feedback snapshot needs this field"),
            });
    }
}
