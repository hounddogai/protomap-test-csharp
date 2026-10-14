namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive payslip review must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePayslipReviewRule
{
    List<string> Check(AdaptivePayslipReview item);
}

public sealed record AdaptivePayslipReviewRequiredCodeRule(string Message) : IAdaptivePayslipReviewRule
{
    public List<string> Check(AdaptivePayslipReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayslipReviewNameLengthRule(int MaxLength) : IAdaptivePayslipReviewRule
{
    public List<string> Check(AdaptivePayslipReview item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayslipReviewEmployeeIdLengthRule(int MaxLength) : IAdaptivePayslipReviewRule
{
    public List<string> Check(AdaptivePayslipReview item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayslipReviewAllowedNameRule(List<string> Allowed) : IAdaptivePayslipReviewRule
{
    public List<string> Check(AdaptivePayslipReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayslipReviewTimezoneLengthRule(int MaxLength) : IAdaptivePayslipReviewRule
{
    public List<string> Check(AdaptivePayslipReview item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive payslip review.</summary>
public sealed record AdaptivePayslipReviewValidator(List<IAdaptivePayslipReviewRule> Rules)
{
    public List<string> Validate(AdaptivePayslipReview item)
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

    public void RequireValid(AdaptivePayslipReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePayslipReview> items)
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

/// <summary>Functions of adaptive payslip review rules.</summary>
public static class AdaptivePayslipReviewRules
{
    public static AdaptivePayslipReviewValidator DefaultAdaptivePayslipReviewValidator()
    {
        return new AdaptivePayslipReviewValidator(
            new List<IAdaptivePayslipReviewRule>
            {
                new AdaptivePayslipReviewRequiredCodeRule("adaptive payslip review needs an entry"),
                new AdaptivePayslipReviewNameLengthRule(255),
                new AdaptivePayslipReviewEmployeeIdLengthRule(120),
                new AdaptivePayslipReviewAllowedNameRule(new List<string> { "global" }),
                new AdaptivePayslipReviewTimezoneLengthRule(255),
            });
    }
}
