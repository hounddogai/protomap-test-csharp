namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant review must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantReviewRule
{
    List<string> Check(EquityGrantReview item);
}

public sealed record EquityGrantReviewBatchSizeRangeRule(int Min, int Max) : IEquityGrantReviewRule
{
    public List<string> Check(EquityGrantReview item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantReviewAllowedCodeRule(List<string> Allowed) : IEquityGrantReviewRule
{
    public List<string> Check(EquityGrantReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantReviewCeilingRangeRule(double Min, double Max) : IEquityGrantReviewRule
{
    public List<string> Check(EquityGrantReview item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantReviewCodeLengthRule(int MaxLength) : IEquityGrantReviewRule
{
    public List<string> Check(EquityGrantReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantReviewReviewerLengthRule(int MaxLength) : IEquityGrantReviewRule
{
    public List<string> Check(EquityGrantReview item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant review.</summary>
public sealed record EquityGrantReviewValidator(List<IEquityGrantReviewRule> Rules)
{
    public List<string> Validate(EquityGrantReview item)
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

    public void RequireValid(EquityGrantReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantReview> items)
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

/// <summary>Functions of equity grant review rules.</summary>
public static class EquityGrantReviewRules
{
    public static EquityGrantReviewValidator DefaultEquityGrantReviewValidator()
    {
        return new EquityGrantReviewValidator(
            new List<IEquityGrantReviewRule>
            {
                new EquityGrantReviewBatchSizeRangeRule(120, 255),
                new EquityGrantReviewAllowedCodeRule(new List<string> { "lenient" }),
                new EquityGrantReviewCeilingRangeRule(9.6, 2952.8),
                new EquityGrantReviewCodeLengthRule(16),
                new EquityGrantReviewReviewerLengthRule(120),
            });
    }
}
