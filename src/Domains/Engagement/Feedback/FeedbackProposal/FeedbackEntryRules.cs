namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a feedback entry must pass. Check returns its problems, or none.</summary>
public interface IFeedbackEntryRule
{
    List<string> Check(FeedbackEntry item);
}

public sealed record FeedbackEntryCodePrefixRule(string Prefix) : IFeedbackEntryRule
{
    public List<string> Check(FeedbackEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackEntryCodeLengthRule(int MaxLength) : IFeedbackEntryRule
{
    public List<string> Check(FeedbackEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackEntryDurationMinutesRangeRule(int Min, int Max) : IFeedbackEntryRule
{
    public List<string> Check(FeedbackEntry item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FeedbackEntryAllowedLabelRule(List<string> Allowed) : IFeedbackEntryRule
{
    public List<string> Check(FeedbackEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a feedback entry.</summary>
public sealed record FeedbackEntryValidator(List<IFeedbackEntryRule> Rules)
{
    public List<string> Validate(FeedbackEntry item)
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

    public void RequireValid(FeedbackEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FeedbackEntry> items)
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

/// <summary>Functions of feedback entry rules.</summary>
public static class FeedbackEntryRules
{
    public static FeedbackEntryValidator DefaultFeedbackEntryValidator()
    {
        return new FeedbackEntryValidator(
            new List<IFeedbackEntryRule>
            {
                new FeedbackEntryCodePrefixRule("ACM"),
                new FeedbackEntryCodeLengthRule(86400),
                new FeedbackEntryDurationMinutesRangeRule(16, 3600),
                new FeedbackEntryAllowedLabelRule(new List<string> { "batched", "fallback", "adaptive" }),
            });
    }
}
