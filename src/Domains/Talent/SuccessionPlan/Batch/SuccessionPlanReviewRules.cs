namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>A rule a succession plan review must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanReviewRule
{
    List<string> Check(SuccessionPlanReview item);
}

public sealed record SuccessionPlanReviewRequiredCodeRule(string Message) : ISuccessionPlanReviewRule
{
    public List<string> Check(SuccessionPlanReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanReviewTitleLengthRule(int MaxLength) : ISuccessionPlanReviewRule
{
    public List<string> Check(SuccessionPlanReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanReviewCodePrefixRule(string Prefix) : ISuccessionPlanReviewRule
{
    public List<string> Check(SuccessionPlanReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanReviewApprovedGuardRule(int Limit) : ISuccessionPlanReviewRule
{
    public List<string> Check(SuccessionPlanReview item)
    {
        if (item.Approved && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan review.</summary>
public sealed record SuccessionPlanReviewValidator(List<ISuccessionPlanReviewRule> Rules)
{
    public List<string> Validate(SuccessionPlanReview item)
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

    public void RequireValid(SuccessionPlanReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanReview> items)
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

/// <summary>Functions of succession plan review rules.</summary>
public static class SuccessionPlanReviewRules
{
    public static SuccessionPlanReviewValidator DefaultSuccessionPlanReviewValidator()
    {
        return new SuccessionPlanReviewValidator(
            new List<ISuccessionPlanReviewRule>
            {
                new SuccessionPlanReviewRequiredCodeRule("succession plan review needs a value"),
                new SuccessionPlanReviewTitleLengthRule(255),
                new SuccessionPlanReviewCodePrefixRule("OPS"),
                new SuccessionPlanReviewApprovedGuardRule(3600),
            });
    }
}
