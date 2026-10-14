namespace Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>A rule a engagement review must pass. Check returns its problems, or none.</summary>
public interface IEngagementReviewRule
{
    List<string> Check(EngagementReview item);
}

public sealed record EngagementReviewRequiredTitleRule(string Message) : IEngagementReviewRule
{
    public List<string> Check(EngagementReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementReviewRequiredCodeRule(string Message) : IEngagementReviewRule
{
    public List<string> Check(EngagementReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementReviewCodePrefixRule(string Prefix) : IEngagementReviewRule
{
    public List<string> Check(EngagementReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementReviewRequiredNameRule(string Message) : IEngagementReviewRule
{
    public List<string> Check(EngagementReview item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementReviewTitleLengthRule(int MaxLength) : IEngagementReviewRule
{
    public List<string> Check(EngagementReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement review.</summary>
public sealed record EngagementReviewValidator(List<IEngagementReviewRule> Rules)
{
    public List<string> Validate(EngagementReview item)
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

    public void RequireValid(EngagementReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementReview> items)
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

/// <summary>Functions of engagement review rules.</summary>
public static class EngagementReviewRules
{
    public static EngagementReviewValidator DefaultEngagementReviewValidator()
    {
        return new EngagementReviewValidator(
            new List<IEngagementReviewRule>
            {
                new EngagementReviewRequiredTitleRule("engagement review needs this field"),
                new EngagementReviewRequiredCodeRule("engagement review needs an entry"),
                new EngagementReviewCodePrefixRule("Q"),
                new EngagementReviewRequiredNameRule("engagement review needs this field"),
                new EngagementReviewTitleLengthRule(255),
            });
    }
}
