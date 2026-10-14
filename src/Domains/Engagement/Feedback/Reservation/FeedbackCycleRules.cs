namespace Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>A rule a feedback cycle must pass. Check returns its problems, or none.</summary>
public interface IFeedbackCycleRule
{
    List<string> Check(FeedbackCycle item);
}

public sealed record FeedbackCycleLeaveDaysRangeRule(int Min, int Max) : IFeedbackCycleRule
{
    public List<string> Check(FeedbackCycle item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackCycleCodePrefixRule(string Prefix) : IFeedbackCycleRule
{
    public List<string> Check(FeedbackCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackCycleCodeLengthRule(int MaxLength) : IFeedbackCycleRule
{
    public List<string> Check(FeedbackCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackCycleTitleLengthRule(int MaxLength) : IFeedbackCycleRule
{
    public List<string> Check(FeedbackCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackCycleRetriesRangeRule(int Min, int Max) : IFeedbackCycleRule
{
    public List<string> Check(FeedbackCycle item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FeedbackCycleArchivedGuardRule(int Limit) : IFeedbackCycleRule
{
    public List<string> Check(FeedbackCycle item)
    {
        if (item.Archived && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback cycle.</summary>
public sealed record FeedbackCycleValidator(List<IFeedbackCycleRule> Rules)
{
    public List<string> Validate(FeedbackCycle item)
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

    public void RequireValid(FeedbackCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackCycle> items)
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

/// <summary>Functions of feedback cycle rules.</summary>
public static class FeedbackCycleRules
{
    public static FeedbackCycleValidator DefaultFeedbackCycleValidator()
    {
        return new FeedbackCycleValidator(
            new List<IFeedbackCycleRule>
            {
                new FeedbackCycleLeaveDaysRangeRule(255, 86400),
                new FeedbackCycleCodePrefixRule("REF"),
                new FeedbackCycleCodeLengthRule(3600),
                new FeedbackCycleTitleLengthRule(3600),
                new FeedbackCycleRetriesRangeRule(80, 80),
                new FeedbackCycleArchivedGuardRule(1000),
            });
    }
}
