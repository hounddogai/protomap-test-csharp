namespace Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>A rule a payslip request must pass. Check returns its problems, or none.</summary>
public interface IPayslipRequestRule
{
    List<string> Check(PayslipRequest item);
}

public sealed record PayslipRequestCodeLengthRule(int MaxLength) : IPayslipRequestRule
{
    public List<string> Check(PayslipRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipRequestReviewerLengthRule(int MaxLength) : IPayslipRequestRule
{
    public List<string> Check(PayslipRequest item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipRequestCategoryLengthRule(int MaxLength) : IPayslipRequestRule
{
    public List<string> Check(PayslipRequest item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipRequestRequiredCategoryRule(string Message) : IPayslipRequestRule
{
    public List<string> Check(PayslipRequest item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipRequestRetriesRangeRule(int Min, int Max) : IPayslipRequestRule
{
    public List<string> Check(PayslipRequest item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a payslip request.</summary>
public sealed record PayslipRequestValidator(List<IPayslipRequestRule> Rules)
{
    public List<string> Validate(PayslipRequest item)
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

    public void RequireValid(PayslipRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipRequest> items)
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

/// <summary>Functions of payslip request rules.</summary>
public static class PayslipRequestRules
{
    public static PayslipRequestValidator DefaultPayslipRequestValidator()
    {
        return new PayslipRequestValidator(
            new List<IPayslipRequestRule>
            {
                new PayslipRequestCodeLengthRule(120),
                new PayslipRequestReviewerLengthRule(3600),
                new PayslipRequestCategoryLengthRule(120),
                new PayslipRequestRequiredCategoryRule("payslip request needs this field"),
                new PayslipRequestRetriesRangeRule(64, 80),
            });
    }
}
