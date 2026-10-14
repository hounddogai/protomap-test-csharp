namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>A rule a pulse survey review must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyReviewRule
{
    List<string> Check(PulseSurveyReview item);
}

public sealed record PulseSurveyReviewSummaryLengthRule(int MaxLength) : IPulseSurveyReviewRule
{
    public List<string> Check(PulseSurveyReview item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyReviewCodePrefixRule(string Prefix) : IPulseSurveyReviewRule
{
    public List<string> Check(PulseSurveyReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyReviewLabelLengthRule(int MaxLength) : IPulseSurveyReviewRule
{
    public List<string> Check(PulseSurveyReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey review.</summary>
public sealed record PulseSurveyReviewValidator(List<IPulseSurveyReviewRule> Rules)
{
    public List<string> Validate(PulseSurveyReview item)
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

    public void RequireValid(PulseSurveyReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyReview> items)
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

/// <summary>Functions of pulse survey review rules.</summary>
public static class PulseSurveyReviewRules
{
    public static PulseSurveyReviewValidator DefaultPulseSurveyReviewValidator()
    {
        return new PulseSurveyReviewValidator(
            new List<IPulseSurveyReviewRule>
            {
                new PulseSurveyReviewSummaryLengthRule(86400),
                new PulseSurveyReviewCodePrefixRule("ACM"),
                new PulseSurveyReviewLabelLengthRule(64),
            });
    }
}
