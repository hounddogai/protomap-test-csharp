namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>A rule a pulse survey entry must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyEntryRule
{
    List<string> Check(PulseSurveyEntry item);
}

public sealed record PulseSurveyEntryCodeLengthRule(int MaxLength) : IPulseSurveyEntryRule
{
    public List<string> Check(PulseSurveyEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyEntryAttemptsRangeRule(int Min, int Max) : IPulseSurveyEntryRule
{
    public List<string> Check(PulseSurveyEntry item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyEntryRequiredReviewerRule(string Message) : IPulseSurveyEntryRule
{
    public List<string> Check(PulseSurveyEntry item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyEntryReviewerLengthRule(int MaxLength) : IPulseSurveyEntryRule
{
    public List<string> Check(PulseSurveyEntry item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyEntryCodePrefixRule(string Prefix) : IPulseSurveyEntryRule
{
    public List<string> Check(PulseSurveyEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyEntryTitleLengthRule(int MaxLength) : IPulseSurveyEntryRule
{
    public List<string> Check(PulseSurveyEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey entry.</summary>
public sealed record PulseSurveyEntryValidator(List<IPulseSurveyEntryRule> Rules)
{
    public List<string> Validate(PulseSurveyEntry item)
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

    public void RequireValid(PulseSurveyEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyEntry> items)
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

/// <summary>Functions of pulse survey entry rules.</summary>
public static class PulseSurveyEntryRules
{
    public static PulseSurveyEntryValidator DefaultPulseSurveyEntryValidator()
    {
        return new PulseSurveyEntryValidator(
            new List<IPulseSurveyEntryRule>
            {
                new PulseSurveyEntryCodeLengthRule(64),
                new PulseSurveyEntryAttemptsRangeRule(120, 86400),
                new PulseSurveyEntryRequiredReviewerRule("pulse survey entry needs this field"),
                new PulseSurveyEntryReviewerLengthRule(64),
                new PulseSurveyEntryCodePrefixRule("X"),
                new PulseSurveyEntryTitleLengthRule(120),
            });
    }
}
