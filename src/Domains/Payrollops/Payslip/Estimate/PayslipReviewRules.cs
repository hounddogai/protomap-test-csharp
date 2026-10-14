namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip review must pass. Check returns its problems, or none.</summary>
public interface IPayslipReviewRule
{
    List<string> Check(PayslipReview item);
}

public sealed record PayslipReviewRequiredTitleRule(string Message) : IPayslipReviewRule
{
    public List<string> Check(PayslipReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipReviewAllowedCodeRule(List<string> Allowed) : IPayslipReviewRule
{
    public List<string> Check(PayslipReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record PayslipReviewRequiredBadgeRule(string Message) : IPayslipReviewRule
{
    public List<string> Check(PayslipReview item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip review.</summary>
public sealed record PayslipReviewValidator(List<IPayslipReviewRule> Rules)
{
    public List<string> Validate(PayslipReview item)
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

    public void RequireValid(PayslipReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipReview> items)
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

/// <summary>Functions of payslip review rules.</summary>
public static class PayslipReviewRules
{
    public static PayslipReviewValidator DefaultPayslipReviewValidator()
    {
        return new PayslipReviewValidator(
            new List<IPayslipReviewRule>
            {
                new PayslipReviewRequiredTitleRule("payslip review needs a value"),
                new PayslipReviewAllowedCodeRule(new List<string> { "weighted" }),
                new PayslipReviewRequiredBadgeRule("payslip review needs an entry"),
            });
    }
}
