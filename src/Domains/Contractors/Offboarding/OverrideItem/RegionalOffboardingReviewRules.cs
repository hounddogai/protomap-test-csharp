namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>A rule a regional offboarding review must pass. Check returns its problems, or none.</summary>
public interface IRegionalOffboardingReviewRule
{
    List<string> Check(RegionalOffboardingReview item);
}

public sealed record RegionalOffboardingReviewRequiredSegmentRule(string Message) : IRegionalOffboardingReviewRule
{
    public List<string> Check(RegionalOffboardingReview item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalOffboardingReviewTitleLengthRule(int MaxLength) : IRegionalOffboardingReviewRule
{
    public List<string> Check(RegionalOffboardingReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalOffboardingReviewLeaveDaysRangeRule(int Min, int Max) : IRegionalOffboardingReviewRule
{
    public List<string> Check(RegionalOffboardingReview item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional offboarding review.</summary>
public sealed record RegionalOffboardingReviewValidator(List<IRegionalOffboardingReviewRule> Rules)
{
    public List<string> Validate(RegionalOffboardingReview item)
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

    public void RequireValid(RegionalOffboardingReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalOffboardingReview> items)
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

/// <summary>Functions of regional offboarding review rules.</summary>
public static class RegionalOffboardingReviewRules
{
    public static RegionalOffboardingReviewValidator DefaultRegionalOffboardingReviewValidator()
    {
        return new RegionalOffboardingReviewValidator(
            new List<IRegionalOffboardingReviewRule>
            {
                new RegionalOffboardingReviewRequiredSegmentRule("regional offboarding review needs a value"),
                new RegionalOffboardingReviewTitleLengthRule(80),
                new RegionalOffboardingReviewLeaveDaysRangeRule(64, 1000),
            });
    }
}
