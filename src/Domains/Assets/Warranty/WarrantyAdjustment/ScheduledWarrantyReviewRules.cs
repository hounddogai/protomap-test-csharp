namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled warranty review must pass. Check returns its problems, or none.</summary>
public interface IScheduledWarrantyReviewRule
{
    List<string> Check(ScheduledWarrantyReview item);
}

public sealed record ScheduledWarrantyReviewRequiredGradeRule(string Message) : IScheduledWarrantyReviewRule
{
    public List<string> Check(ScheduledWarrantyReview item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledWarrantyReviewAllowedTitleRule(List<string> Allowed) : IScheduledWarrantyReviewRule
{
    public List<string> Check(ScheduledWarrantyReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledWarrantyReviewCodePrefixRule(string Prefix) : IScheduledWarrantyReviewRule
{
    public List<string> Check(ScheduledWarrantyReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledWarrantyReviewRequiredCodeRule(string Message) : IScheduledWarrantyReviewRule
{
    public List<string> Check(ScheduledWarrantyReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled warranty review.</summary>
public sealed record ScheduledWarrantyReviewValidator(List<IScheduledWarrantyReviewRule> Rules)
{
    public List<string> Validate(ScheduledWarrantyReview item)
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

    public void RequireValid(ScheduledWarrantyReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledWarrantyReview> items)
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

/// <summary>Functions of scheduled warranty review rules.</summary>
public static class ScheduledWarrantyReviewRules
{
    public static ScheduledWarrantyReviewValidator DefaultScheduledWarrantyReviewValidator()
    {
        return new ScheduledWarrantyReviewValidator(
            new List<IScheduledWarrantyReviewRule>
            {
                new ScheduledWarrantyReviewRequiredGradeRule("scheduled warranty review needs a value"),
                new ScheduledWarrantyReviewAllowedTitleRule(new List<string> { "capped", "fallback", "batched" }),
                new ScheduledWarrantyReviewCodePrefixRule("Q"),
                new ScheduledWarrantyReviewRequiredCodeRule("scheduled warranty review needs a value"),
            });
    }
}
