namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback reservation must pass. Check returns its problems, or none.</summary>
public interface IFeedbackReservationRule
{
    List<string> Check(FeedbackReservation item);
}

public sealed record FeedbackReservationRequiredLabelRule(string Message) : IFeedbackReservationRule
{
    public List<string> Check(FeedbackReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackReservationRequiredCodeRule(string Message) : IFeedbackReservationRule
{
    public List<string> Check(FeedbackReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FeedbackReservationEndsAtRangeRule(int Min, int Max) : IFeedbackReservationRule
{
    public List<string> Check(FeedbackReservation item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FeedbackReservationAllowedCodeRule(List<string> Allowed) : IFeedbackReservationRule
{
    public List<string> Check(FeedbackReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback reservation.</summary>
public sealed record FeedbackReservationValidator(List<IFeedbackReservationRule> Rules)
{
    public List<string> Validate(FeedbackReservation item)
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

    public void RequireValid(FeedbackReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackReservation> items)
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

/// <summary>Functions of feedback reservation rules.</summary>
public static class FeedbackReservationRules
{
    public static FeedbackReservationValidator DefaultFeedbackReservationValidator()
    {
        return new FeedbackReservationValidator(
            new List<IFeedbackReservationRule>
            {
                new FeedbackReservationRequiredLabelRule("feedback reservation needs a value"),
                new FeedbackReservationRequiredCodeRule("feedback reservation needs an entry"),
                new FeedbackReservationEndsAtRangeRule(32, 255),
                new FeedbackReservationAllowedCodeRule(new List<string> { "tiered" }),
            });
    }
}
