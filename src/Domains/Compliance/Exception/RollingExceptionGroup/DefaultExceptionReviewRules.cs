namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>A rule a default exception review must pass. Check returns its problems, or none.</summary>
public interface IDefaultExceptionReviewRule
{
    List<string> Check(DefaultExceptionReview item);
}

public sealed record DefaultExceptionReviewLabelLengthRule(int MaxLength) : IDefaultExceptionReviewRule
{
    public List<string> Check(DefaultExceptionReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultExceptionReviewCodeLengthRule(int MaxLength) : IDefaultExceptionReviewRule
{
    public List<string> Check(DefaultExceptionReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultExceptionReviewCapacityRangeRule(int Min, int Max) : IDefaultExceptionReviewRule
{
    public List<string> Check(DefaultExceptionReview item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default exception review.</summary>
public sealed record DefaultExceptionReviewValidator(List<IDefaultExceptionReviewRule> Rules)
{
    public List<string> Validate(DefaultExceptionReview item)
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

    public void RequireValid(DefaultExceptionReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultExceptionReview> items)
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

/// <summary>Functions of default exception review rules.</summary>
public static class DefaultExceptionReviewRules
{
    public static DefaultExceptionReviewValidator DefaultDefaultExceptionReviewValidator()
    {
        return new DefaultExceptionReviewValidator(
            new List<IDefaultExceptionReviewRule>
            {
                new DefaultExceptionReviewLabelLengthRule(3600),
                new DefaultExceptionReviewCodeLengthRule(255),
                new DefaultExceptionReviewCapacityRangeRule(255, 255),
            });
    }
}
