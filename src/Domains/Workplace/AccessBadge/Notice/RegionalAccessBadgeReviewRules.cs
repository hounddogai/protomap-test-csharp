namespace Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>A rule a regional access badge review must pass. Check returns its problems, or none.</summary>
public interface IRegionalAccessBadgeReviewRule
{
    List<string> Check(RegionalAccessBadgeReview item);
}

public sealed record RegionalAccessBadgeReviewRequiredDepartmentRule(string Message) : IRegionalAccessBadgeReviewRule
{
    public List<string> Check(RegionalAccessBadgeReview item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccessBadgeReviewCodeLengthRule(int MaxLength) : IRegionalAccessBadgeReviewRule
{
    public List<string> Check(RegionalAccessBadgeReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalAccessBadgeReviewLabelLengthRule(int MaxLength) : IRegionalAccessBadgeReviewRule
{
    public List<string> Check(RegionalAccessBadgeReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional access badge review.</summary>
public sealed record RegionalAccessBadgeReviewValidator(List<IRegionalAccessBadgeReviewRule> Rules)
{
    public List<string> Validate(RegionalAccessBadgeReview item)
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

    public void RequireValid(RegionalAccessBadgeReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalAccessBadgeReview> items)
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

/// <summary>Functions of regional access badge review rules.</summary>
public static class RegionalAccessBadgeReviewRules
{
    public static RegionalAccessBadgeReviewValidator DefaultRegionalAccessBadgeReviewValidator()
    {
        return new RegionalAccessBadgeReviewValidator(
            new List<IRegionalAccessBadgeReviewRule>
            {
                new RegionalAccessBadgeReviewRequiredDepartmentRule("regional access badge review needs an entry"),
                new RegionalAccessBadgeReviewCodeLengthRule(1000),
                new RegionalAccessBadgeReviewLabelLengthRule(1000),
            });
    }
}
