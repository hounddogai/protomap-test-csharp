namespace Acme.Workforce.Payrollops.Payslip.Batch;

/// <summary>A rule a default payslip review must pass. Check returns its problems, or none.</summary>
public interface IDefaultPayslipReviewRule
{
    List<string> Check(DefaultPayslipReview item);
}

public sealed record DefaultPayslipReviewRevisionRangeRule(int Min, int Max) : IDefaultPayslipReviewRule
{
    public List<string> Check(DefaultPayslipReview item)
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

public sealed record DefaultPayslipReviewWindowRule(int MaxSpan) : IDefaultPayslipReviewRule
{
    public List<string> Check(DefaultPayslipReview item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayslipReviewMinimumRangeRule(double Min, double Max) : IDefaultPayslipReviewRule
{
    public List<string> Check(DefaultPayslipReview item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default payslip review.</summary>
public sealed record DefaultPayslipReviewValidator(List<IDefaultPayslipReviewRule> Rules)
{
    public List<string> Validate(DefaultPayslipReview item)
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

    public void RequireValid(DefaultPayslipReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPayslipReview> items)
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

/// <summary>Functions of default payslip review rules.</summary>
public static class DefaultPayslipReviewRules
{
    public static DefaultPayslipReviewValidator DefaultDefaultPayslipReviewValidator()
    {
        return new DefaultPayslipReviewValidator(
            new List<IDefaultPayslipReviewRule>
            {
                new DefaultPayslipReviewRevisionRangeRule(16, 32),
                new DefaultPayslipReviewWindowRule(32),
                new DefaultPayslipReviewMinimumRangeRule(8.5, 674.6),
            });
    }
}
