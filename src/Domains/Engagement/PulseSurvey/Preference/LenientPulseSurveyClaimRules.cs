namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>A rule a lenient pulse survey claim must pass. Check returns its problems, or none.</summary>
public interface ILenientPulseSurveyClaimRule
{
    List<string> Check(LenientPulseSurveyClaim item);
}

public sealed record LenientPulseSurveyClaimCodeLengthRule(int MaxLength) : ILenientPulseSurveyClaimRule
{
    public List<string> Check(LenientPulseSurveyClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPulseSurveyClaimTitleLengthRule(int MaxLength) : ILenientPulseSurveyClaimRule
{
    public List<string> Check(LenientPulseSurveyClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPulseSurveyClaimDescriptionLengthRule(int MaxLength) : ILenientPulseSurveyClaimRule
{
    public List<string> Check(LenientPulseSurveyClaim item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPulseSurveyClaimRequiredTitleRule(string Message) : ILenientPulseSurveyClaimRule
{
    public List<string> Check(LenientPulseSurveyClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPulseSurveyClaimRequiredNotesRule(string Message) : ILenientPulseSurveyClaimRule
{
    public List<string> Check(LenientPulseSurveyClaim item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPulseSurveyClaimCodePrefixRule(string Prefix) : ILenientPulseSurveyClaimRule
{
    public List<string> Check(LenientPulseSurveyClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient pulse survey claim.</summary>
public sealed record LenientPulseSurveyClaimValidator(List<ILenientPulseSurveyClaimRule> Rules)
{
    public List<string> Validate(LenientPulseSurveyClaim item)
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

    public void RequireValid(LenientPulseSurveyClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPulseSurveyClaim> items)
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

/// <summary>Functions of lenient pulse survey claim rules.</summary>
public static class LenientPulseSurveyClaimRules
{
    public static LenientPulseSurveyClaimValidator DefaultLenientPulseSurveyClaimValidator()
    {
        return new LenientPulseSurveyClaimValidator(
            new List<ILenientPulseSurveyClaimRule>
            {
                new LenientPulseSurveyClaimCodeLengthRule(16),
                new LenientPulseSurveyClaimTitleLengthRule(255),
                new LenientPulseSurveyClaimDescriptionLengthRule(255),
                new LenientPulseSurveyClaimRequiredTitleRule("lenient pulse survey claim needs an entry"),
                new LenientPulseSurveyClaimRequiredNotesRule("lenient pulse survey claim needs a value"),
                new LenientPulseSurveyClaimCodePrefixRule("X"),
            });
    }
}
