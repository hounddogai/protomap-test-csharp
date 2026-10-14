namespace Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>A rule a feedback record must pass. Check returns its problems, or none.</summary>
public interface IFeedbackRecordRule
{
    List<string> Check(FeedbackRecord item);
}

public sealed record FeedbackRecordDescriptionLengthRule(int MaxLength) : IFeedbackRecordRule
{
    public List<string> Check(FeedbackRecord item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackRecordRequiredLabelRule(string Message) : IFeedbackRecordRule
{
    public List<string> Check(FeedbackRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackRecordCodePrefixRule(string Prefix) : IFeedbackRecordRule
{
    public List<string> Check(FeedbackRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackRecordLabelLengthRule(int MaxLength) : IFeedbackRecordRule
{
    public List<string> Check(FeedbackRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackRecordRequiredTeamRule(string Message) : IFeedbackRecordRule
{
    public List<string> Check(FeedbackRecord item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackRecordLeaveDaysRangeRule(int Min, int Max) : IFeedbackRecordRule
{
    public List<string> Check(FeedbackRecord item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a feedback record.</summary>
public sealed record FeedbackRecordValidator(List<IFeedbackRecordRule> Rules)
{
    public List<string> Validate(FeedbackRecord item)
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

    public void RequireValid(FeedbackRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackRecord> items)
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

/// <summary>Functions of feedback record rules.</summary>
public static class FeedbackRecordRules
{
    public static FeedbackRecordValidator DefaultFeedbackRecordValidator()
    {
        return new FeedbackRecordValidator(
            new List<IFeedbackRecordRule>
            {
                new FeedbackRecordDescriptionLengthRule(86400),
                new FeedbackRecordRequiredLabelRule("feedback record needs an entry"),
                new FeedbackRecordCodePrefixRule("Q"),
                new FeedbackRecordLabelLengthRule(86400),
                new FeedbackRecordRequiredTeamRule("feedback record needs this field"),
                new FeedbackRecordLeaveDaysRangeRule(64, 64),
            });
    }
}
