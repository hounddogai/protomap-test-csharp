namespace Acme.Workforce.Engagement.Feedback.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback feedback entry must pass. Check returns its problems, or none.</summary>
public interface IFallbackFeedbackEntryRule
{
    List<string> Check(FallbackFeedbackEntry item);
}

public sealed record FallbackFeedbackEntryCodeLengthRule(int MaxLength) : IFallbackFeedbackEntryRule
{
    public List<string> Check(FallbackFeedbackEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackFeedbackEntryTeamLengthRule(int MaxLength) : IFallbackFeedbackEntryRule
{
    public List<string> Check(FallbackFeedbackEntry item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackFeedbackEntryLeaveDaysRangeRule(int Min, int Max) : IFallbackFeedbackEntryRule
{
    public List<string> Check(FallbackFeedbackEntry item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackFeedbackEntryAllowedTeamRule(List<string> Allowed) : IFallbackFeedbackEntryRule
{
    public List<string> Check(FallbackFeedbackEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is stale" };
        }
        return new List<string>();
    }
}

public sealed record FallbackFeedbackEntryLabelLengthRule(int MaxLength) : IFallbackFeedbackEntryRule
{
    public List<string> Check(FallbackFeedbackEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackFeedbackEntryRequiredPhoneRule(string Message) : IFallbackFeedbackEntryRule
{
    public List<string> Check(FallbackFeedbackEntry item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback feedback entry.</summary>
public sealed record FallbackFeedbackEntryValidator(List<IFallbackFeedbackEntryRule> Rules)
{
    public List<string> Validate(FallbackFeedbackEntry item)
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

    public void RequireValid(FallbackFeedbackEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackFeedbackEntry> items)
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

/// <summary>Functions of fallback feedback entry rules.</summary>
public static class FallbackFeedbackEntryRules
{
    public static FallbackFeedbackEntryValidator DefaultFallbackFeedbackEntryValidator()
    {
        return new FallbackFeedbackEntryValidator(
            new List<IFallbackFeedbackEntryRule>
            {
                new FallbackFeedbackEntryCodeLengthRule(255),
                new FallbackFeedbackEntryTeamLengthRule(32),
                new FallbackFeedbackEntryLeaveDaysRangeRule(64, 3600),
                new FallbackFeedbackEntryAllowedTeamRule(new List<string> { "manual" }),
                new FallbackFeedbackEntryLabelLengthRule(120),
                new FallbackFeedbackEntryRequiredPhoneRule("fallback feedback entry needs this field"),
            });
    }
}
