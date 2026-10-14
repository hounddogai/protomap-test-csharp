namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a action plan review must pass. Check returns its problems, or none.</summary>
public interface IActionPlanReviewRule
{
    List<string> Check(ActionPlanReview item);
}

public sealed record ActionPlanReviewMaximumRangeRule(double Min, double Max) : IActionPlanReviewRule
{
    public List<string> Check(ActionPlanReview item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanReviewAllowedCodeRule(List<string> Allowed) : IActionPlanReviewRule
{
    public List<string> Check(ActionPlanReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanReviewLabelLengthRule(int MaxLength) : IActionPlanReviewRule
{
    public List<string> Check(ActionPlanReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanReviewCategoryLengthRule(int MaxLength) : IActionPlanReviewRule
{
    public List<string> Check(ActionPlanReview item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanReviewRequiredCategoryRule(string Message) : IActionPlanReviewRule
{
    public List<string> Check(ActionPlanReview item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan review.</summary>
public sealed record ActionPlanReviewValidator(List<IActionPlanReviewRule> Rules)
{
    public List<string> Validate(ActionPlanReview item)
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

    public void RequireValid(ActionPlanReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanReview> items)
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

/// <summary>Functions of action plan review rules.</summary>
public static class ActionPlanReviewRules
{
    public static ActionPlanReviewValidator DefaultActionPlanReviewValidator()
    {
        return new ActionPlanReviewValidator(
            new List<IActionPlanReviewRule>
            {
                new ActionPlanReviewMaximumRangeRule(6.7, 1729.6),
                new ActionPlanReviewAllowedCodeRule(new List<string> { "batched" }),
                new ActionPlanReviewLabelLengthRule(16),
                new ActionPlanReviewCategoryLengthRule(255),
                new ActionPlanReviewRequiredCategoryRule("action plan review needs this field"),
            });
    }
}
