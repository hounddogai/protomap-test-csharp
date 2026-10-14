namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional mobility review must pass. Check returns its problems, or none.</summary>
public interface IRegionalMobilityReviewRule
{
    List<string> Check(RegionalMobilityReview item);
}

public sealed record RegionalMobilityReviewAllowedDepartmentRule(List<string> Allowed) : IRegionalMobilityReviewRule
{
    public List<string> Check(RegionalMobilityReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record RegionalMobilityReviewVisibleGuardRule(int Limit) : IRegionalMobilityReviewRule
{
    public List<string> Check(RegionalMobilityReview item)
    {
        if (item.Visible && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record RegionalMobilityReviewRequiredCodeRule(string Message) : IRegionalMobilityReviewRule
{
    public List<string> Check(RegionalMobilityReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalMobilityReviewRequiredTitleRule(string Message) : IRegionalMobilityReviewRule
{
    public List<string> Check(RegionalMobilityReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional mobility review.</summary>
public sealed record RegionalMobilityReviewValidator(List<IRegionalMobilityReviewRule> Rules)
{
    public List<string> Validate(RegionalMobilityReview item)
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

    public void RequireValid(RegionalMobilityReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalMobilityReview> items)
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

/// <summary>Functions of regional mobility review rules.</summary>
public static class RegionalMobilityReviewRules
{
    public static RegionalMobilityReviewValidator DefaultRegionalMobilityReviewValidator()
    {
        return new RegionalMobilityReviewValidator(
            new List<IRegionalMobilityReviewRule>
            {
                new RegionalMobilityReviewAllowedDepartmentRule(new List<string>()),
                new RegionalMobilityReviewVisibleGuardRule(255),
                new RegionalMobilityReviewRequiredCodeRule("regional mobility review needs an entry"),
                new RegionalMobilityReviewRequiredTitleRule("regional mobility review needs an entry"),
            });
    }
}
