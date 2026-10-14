namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched bonus pool review must pass. Check returns its problems, or none.</summary>
public interface IBatchedBonusPoolReviewRule
{
    List<string> Check(BatchedBonusPoolReview item);
}

public sealed record BatchedBonusPoolReviewAllowedCodeRule(List<string> Allowed) : IBatchedBonusPoolReviewRule
{
    public List<string> Check(BatchedBonusPoolReview item)
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

public sealed record BatchedBonusPoolReviewRevisionRangeRule(int Min, int Max) : IBatchedBonusPoolReviewRule
{
    public List<string> Check(BatchedBonusPoolReview item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedBonusPoolReviewTitleLengthRule(int MaxLength) : IBatchedBonusPoolReviewRule
{
    public List<string> Check(BatchedBonusPoolReview item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBonusPoolReviewHoursRangeRule(double Min, double Max) : IBatchedBonusPoolReviewRule
{
    public List<string> Check(BatchedBonusPoolReview item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedBonusPoolReviewRequiredManagerIdRule(string Message) : IBatchedBonusPoolReviewRule
{
    public List<string> Check(BatchedBonusPoolReview item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedBonusPoolReviewExternalRefLengthRule(int MaxLength) : IBatchedBonusPoolReviewRule
{
    public List<string> Check(BatchedBonusPoolReview item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched bonus pool review.</summary>
public sealed record BatchedBonusPoolReviewValidator(List<IBatchedBonusPoolReviewRule> Rules)
{
    public List<string> Validate(BatchedBonusPoolReview item)
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

    public void RequireValid(BatchedBonusPoolReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedBonusPoolReview> items)
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

/// <summary>Functions of batched bonus pool review rules.</summary>
public static class BatchedBonusPoolReviewRules
{
    public static BatchedBonusPoolReviewValidator DefaultBatchedBonusPoolReviewValidator()
    {
        return new BatchedBonusPoolReviewValidator(
            new List<IBatchedBonusPoolReviewRule>
            {
                new BatchedBonusPoolReviewAllowedCodeRule(new List<string> { "regional" }),
                new BatchedBonusPoolReviewRevisionRangeRule(32, 64),
                new BatchedBonusPoolReviewTitleLengthRule(120),
                new BatchedBonusPoolReviewHoursRangeRule(8.5, 3181.4),
                new BatchedBonusPoolReviewRequiredManagerIdRule("batched bonus pool review needs a value"),
                new BatchedBonusPoolReviewExternalRefLengthRule(80),
            });
    }
}
