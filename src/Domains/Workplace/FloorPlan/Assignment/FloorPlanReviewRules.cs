namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan review must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanReviewRule
{
    List<string> Check(FloorPlanReview item);
}

public sealed record FloorPlanReviewAllowedCategoryRule(List<string> Allowed) : IFloorPlanReviewRule
{
    public List<string> Check(FloorPlanReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanReviewCategoryLengthRule(int MaxLength) : IFloorPlanReviewRule
{
    public List<string> Check(FloorPlanReview item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanReviewRequiredCategoryRule(string Message) : IFloorPlanReviewRule
{
    public List<string> Check(FloorPlanReview item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanReviewRequiredNameRule(string Message) : IFloorPlanReviewRule
{
    public List<string> Check(FloorPlanReview item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan review.</summary>
public sealed record FloorPlanReviewValidator(List<IFloorPlanReviewRule> Rules)
{
    public List<string> Validate(FloorPlanReview item)
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

    public void RequireValid(FloorPlanReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanReview> items)
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

/// <summary>Functions of floor plan review rules.</summary>
public static class FloorPlanReviewRules
{
    public static FloorPlanReviewValidator DefaultFloorPlanReviewValidator()
    {
        return new FloorPlanReviewValidator(
            new List<IFloorPlanReviewRule>
            {
                new FloorPlanReviewAllowedCategoryRule(new List<string> { "scheduled", "tiered", "seasonal" }),
                new FloorPlanReviewCategoryLengthRule(3600),
                new FloorPlanReviewRequiredCategoryRule("floor plan review needs an entry"),
                new FloorPlanReviewRequiredNameRule("floor plan review needs an entry"),
            });
    }
}
