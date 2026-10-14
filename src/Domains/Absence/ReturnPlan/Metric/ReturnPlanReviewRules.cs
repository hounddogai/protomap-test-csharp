namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a return plan review must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanReviewRule
{
    List<string> Check(ReturnPlanReview item);
}

public sealed record ReturnPlanReviewBatchSizeRangeRule(int Min, int Max) : IReturnPlanReviewRule
{
    public List<string> Check(ReturnPlanReview item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanReviewAllowedCodeRule(List<string> Allowed) : IReturnPlanReviewRule
{
    public List<string> Check(ReturnPlanReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanReviewBudgetRangeRule(double Min, double Max) : IReturnPlanReviewRule
{
    public List<string> Check(ReturnPlanReview item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan review.</summary>
public sealed record ReturnPlanReviewValidator(List<IReturnPlanReviewRule> Rules)
{
    public List<string> Validate(ReturnPlanReview item)
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

    public void RequireValid(ReturnPlanReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanReview> items)
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

/// <summary>Functions of return plan review rules.</summary>
public static class ReturnPlanReviewRules
{
    public static ReturnPlanReviewValidator DefaultReturnPlanReviewValidator()
    {
        return new ReturnPlanReviewValidator(
            new List<IReturnPlanReviewRule>
            {
                new ReturnPlanReviewBatchSizeRangeRule(32, 1000),
                new ReturnPlanReviewAllowedCodeRule(new List<string> { "weighted" }),
                new ReturnPlanReviewBudgetRangeRule(2.2, 2516.0),
            });
    }
}
