namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>A rule a fallback feedback reservation must pass. Check returns its problems, or none.</summary>
public interface IFallbackFeedbackReservationRule
{
    List<string> Check(FallbackFeedbackReservation item);
}

public sealed record FallbackFeedbackReservationChecksumLengthRule(int MaxLength) : IFallbackFeedbackReservationRule
{
    public List<string> Check(FallbackFeedbackReservation item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackFeedbackReservationRequiredCodeRule(string Message) : IFallbackFeedbackReservationRule
{
    public List<string> Check(FallbackFeedbackReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackFeedbackReservationLabelLengthRule(int MaxLength) : IFallbackFeedbackReservationRule
{
    public List<string> Check(FallbackFeedbackReservation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback feedback reservation.</summary>
public sealed record FallbackFeedbackReservationValidator(List<IFallbackFeedbackReservationRule> Rules)
{
    public List<string> Validate(FallbackFeedbackReservation item)
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

    public void RequireValid(FallbackFeedbackReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackFeedbackReservation> items)
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

/// <summary>Functions of fallback feedback reservation rules.</summary>
public static class FallbackFeedbackReservationRules
{
    public static FallbackFeedbackReservationValidator DefaultFallbackFeedbackReservationValidator()
    {
        return new FallbackFeedbackReservationValidator(
            new List<IFallbackFeedbackReservationRule>
            {
                new FallbackFeedbackReservationChecksumLengthRule(64),
                new FallbackFeedbackReservationRequiredCodeRule("fallback feedback reservation needs this field"),
                new FallbackFeedbackReservationLabelLengthRule(32),
            });
    }
}
