namespace Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>A rule a seasonal feedback claim must pass. Check returns its problems, or none.</summary>
public interface ISeasonalFeedbackClaimRule
{
    List<string> Check(SeasonalFeedbackClaim item);
}

public sealed record SeasonalFeedbackClaimRequiredSalaryBandRule(string Message) : ISeasonalFeedbackClaimRule
{
    public List<string> Check(SeasonalFeedbackClaim item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackClaimLimitRangeRule(int Min, int Max) : ISeasonalFeedbackClaimRule
{
    public List<string> Check(SeasonalFeedbackClaim item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackClaimEndsAtRangeRule(int Min, int Max) : ISeasonalFeedbackClaimRule
{
    public List<string> Check(SeasonalFeedbackClaim item)
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

public sealed record SeasonalFeedbackClaimRequiredReviewerRule(string Message) : ISeasonalFeedbackClaimRule
{
    public List<string> Check(SeasonalFeedbackClaim item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalFeedbackClaimCodePrefixRule(string Prefix) : ISeasonalFeedbackClaimRule
{
    public List<string> Check(SeasonalFeedbackClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal feedback claim.</summary>
public sealed record SeasonalFeedbackClaimValidator(List<ISeasonalFeedbackClaimRule> Rules)
{
    public List<string> Validate(SeasonalFeedbackClaim item)
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

    public void RequireValid(SeasonalFeedbackClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalFeedbackClaim> items)
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

/// <summary>Functions of seasonal feedback claim rules.</summary>
public static class SeasonalFeedbackClaimRules
{
    public static SeasonalFeedbackClaimValidator DefaultSeasonalFeedbackClaimValidator()
    {
        return new SeasonalFeedbackClaimValidator(
            new List<ISeasonalFeedbackClaimRule>
            {
                new SeasonalFeedbackClaimRequiredSalaryBandRule("seasonal feedback claim needs a value"),
                new SeasonalFeedbackClaimLimitRangeRule(16, 3600),
                new SeasonalFeedbackClaimEndsAtRangeRule(64, 120),
                new SeasonalFeedbackClaimRequiredReviewerRule("seasonal feedback claim needs this field"),
                new SeasonalFeedbackClaimCodePrefixRule("Q"),
            });
    }
}
