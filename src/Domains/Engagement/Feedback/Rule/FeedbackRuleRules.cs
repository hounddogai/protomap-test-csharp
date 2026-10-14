namespace Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>A rule a feedback rule must pass. Check returns its problems, or none.</summary>
public interface IFeedbackRuleRule
{
    List<string> Check(FeedbackRule item);
}

public sealed record FeedbackRuleOwnerIdLengthRule(int MaxLength) : IFeedbackRuleRule
{
    public List<string> Check(FeedbackRule item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackRuleRequiredOwnerIdRule(string Message) : IFeedbackRuleRule
{
    public List<string> Check(FeedbackRule item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackRulePinnedGuardRule(int Limit) : IFeedbackRuleRule
{
    public List<string> Check(FeedbackRule item)
    {
        if (item.Pinned && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackRuleRequiredCodeRule(string Message) : IFeedbackRuleRule
{
    public List<string> Check(FeedbackRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackRuleRequiredNotesRule(string Message) : IFeedbackRuleRule
{
    public List<string> Check(FeedbackRule item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback rule.</summary>
public sealed record FeedbackRuleValidator(List<IFeedbackRuleRule> Rules)
{
    public List<string> Validate(FeedbackRule item)
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

    public void RequireValid(FeedbackRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackRule> items)
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

/// <summary>Functions of feedback rule rules.</summary>
public static class FeedbackRuleRules
{
    public static FeedbackRuleValidator DefaultFeedbackRuleValidator()
    {
        return new FeedbackRuleValidator(
            new List<IFeedbackRuleRule>
            {
                new FeedbackRuleOwnerIdLengthRule(32),
                new FeedbackRuleRequiredOwnerIdRule("feedback rule needs a value"),
                new FeedbackRulePinnedGuardRule(32),
                new FeedbackRuleRequiredCodeRule("feedback rule needs an entry"),
                new FeedbackRuleRequiredNotesRule("feedback rule needs a value"),
            });
    }
}
