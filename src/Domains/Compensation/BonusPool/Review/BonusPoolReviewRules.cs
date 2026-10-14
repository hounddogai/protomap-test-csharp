namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a bonus pool review must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolReviewRule
{
    List<string> Check(BonusPoolReview item);
}

public sealed record BonusPoolReviewAllowedTitleRule(List<string> Allowed) : IBonusPoolReviewRule
{
    public List<string> Check(BonusPoolReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolReviewRequiredCodeRule(string Message) : IBonusPoolReviewRule
{
    public List<string> Check(BonusPoolReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolReviewLabelLengthRule(int MaxLength) : IBonusPoolReviewRule
{
    public List<string> Check(BonusPoolReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool review.</summary>
public sealed record BonusPoolReviewValidator(List<IBonusPoolReviewRule> Rules)
{
    public List<string> Validate(BonusPoolReview item)
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

    public void RequireValid(BonusPoolReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolReview> items)
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

/// <summary>Functions of bonus pool review rules.</summary>
public static class BonusPoolReviewRules
{
    public static BonusPoolReviewValidator DefaultBonusPoolReviewValidator()
    {
        return new BonusPoolReviewValidator(
            new List<IBonusPoolReviewRule>
            {
                new BonusPoolReviewAllowedTitleRule(new List<string>()),
                new BonusPoolReviewRequiredCodeRule("bonus pool review needs this field"),
                new BonusPoolReviewLabelLengthRule(1000),
            });
    }
}
