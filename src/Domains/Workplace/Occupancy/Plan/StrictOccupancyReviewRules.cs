namespace Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>A rule a strict occupancy review must pass. Check returns its problems, or none.</summary>
public interface IStrictOccupancyReviewRule
{
    List<string> Check(StrictOccupancyReview item);
}

public sealed record StrictOccupancyReviewRequiredCodeRule(string Message) : IStrictOccupancyReviewRule
{
    public List<string> Check(StrictOccupancyReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancyReviewCodePrefixRule(string Prefix) : IStrictOccupancyReviewRule
{
    public List<string> Check(StrictOccupancyReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancyReviewRequiredCostCenterRule(string Message) : IStrictOccupancyReviewRule
{
    public List<string> Check(StrictOccupancyReview item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancyReviewRequiredSkillRule(string Message) : IStrictOccupancyReviewRule
{
    public List<string> Check(StrictOccupancyReview item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict occupancy review.</summary>
public sealed record StrictOccupancyReviewValidator(List<IStrictOccupancyReviewRule> Rules)
{
    public List<string> Validate(StrictOccupancyReview item)
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

    public void RequireValid(StrictOccupancyReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictOccupancyReview> items)
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

/// <summary>Functions of strict occupancy review rules.</summary>
public static class StrictOccupancyReviewRules
{
    public static StrictOccupancyReviewValidator DefaultStrictOccupancyReviewValidator()
    {
        return new StrictOccupancyReviewValidator(
            new List<IStrictOccupancyReviewRule>
            {
                new StrictOccupancyReviewRequiredCodeRule("strict occupancy review needs a value"),
                new StrictOccupancyReviewCodePrefixRule("Q"),
                new StrictOccupancyReviewRequiredCostCenterRule("strict occupancy review needs this field"),
                new StrictOccupancyReviewRequiredSkillRule("strict occupancy review needs an entry"),
            });
    }
}
