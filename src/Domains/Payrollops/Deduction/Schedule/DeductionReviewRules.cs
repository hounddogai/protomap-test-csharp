namespace Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>A rule a deduction review must pass. Check returns its problems, or none.</summary>
public interface IDeductionReviewRule
{
    List<string> Check(DeductionReview item);
}

public sealed record DeductionReviewLabelLengthRule(int MaxLength) : IDeductionReviewRule
{
    public List<string> Check(DeductionReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionReviewRevisionRangeRule(int Min, int Max) : IDeductionReviewRule
{
    public List<string> Check(DeductionReview item)
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

public sealed record DeductionReviewRequiredDepartmentRule(string Message) : IDeductionReviewRule
{
    public List<string> Check(DeductionReview item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionReviewRequiredCodeRule(string Message) : IDeductionReviewRule
{
    public List<string> Check(DeductionReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionReviewRequiredLabelRule(string Message) : IDeductionReviewRule
{
    public List<string> Check(DeductionReview item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionReviewDepartmentLengthRule(int MaxLength) : IDeductionReviewRule
{
    public List<string> Check(DeductionReview item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction review.</summary>
public sealed record DeductionReviewValidator(List<IDeductionReviewRule> Rules)
{
    public List<string> Validate(DeductionReview item)
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

    public void RequireValid(DeductionReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionReview> items)
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

/// <summary>Functions of deduction review rules.</summary>
public static class DeductionReviewRules
{
    public static DeductionReviewValidator DefaultDeductionReviewValidator()
    {
        return new DeductionReviewValidator(
            new List<IDeductionReviewRule>
            {
                new DeductionReviewLabelLengthRule(16),
                new DeductionReviewRevisionRangeRule(64, 255),
                new DeductionReviewRequiredDepartmentRule("deduction review needs a value"),
                new DeductionReviewRequiredCodeRule("deduction review needs this field"),
                new DeductionReviewRequiredLabelRule("deduction review needs an entry"),
                new DeductionReviewDepartmentLengthRule(120),
            });
    }
}
