namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip draft must pass. Check returns its problems, or none.</summary>
public interface IPayslipDraftRule
{
    List<string> Check(PayslipDraft item);
}

public sealed record PayslipDraftRequiredCodeRule(string Message) : IPayslipDraftRule
{
    public List<string> Check(PayslipDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipDraftAllowedNameRule(List<string> Allowed) : IPayslipDraftRule
{
    public List<string> Check(PayslipDraft item)
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

public sealed record PayslipDraftCodeLengthRule(int MaxLength) : IPayslipDraftRule
{
    public List<string> Check(PayslipDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipDraftNameLengthRule(int MaxLength) : IPayslipDraftRule
{
    public List<string> Check(PayslipDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipDraftRatingRangeRule(double Min, double Max) : IPayslipDraftRule
{
    public List<string> Check(PayslipDraft item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipDraftReviewerLengthRule(int MaxLength) : IPayslipDraftRule
{
    public List<string> Check(PayslipDraft item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip draft.</summary>
public sealed record PayslipDraftValidator(List<IPayslipDraftRule> Rules)
{
    public List<string> Validate(PayslipDraft item)
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

    public void RequireValid(PayslipDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipDraft> items)
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

/// <summary>Functions of payslip draft rules.</summary>
public static class PayslipDraftRules
{
    public static PayslipDraftValidator DefaultPayslipDraftValidator()
    {
        return new PayslipDraftValidator(
            new List<IPayslipDraftRule>
            {
                new PayslipDraftRequiredCodeRule("payslip draft needs an entry"),
                new PayslipDraftAllowedNameRule(new List<string> { "strict", "regional", "seasonal" }),
                new PayslipDraftCodeLengthRule(3600),
                new PayslipDraftNameLengthRule(1000),
                new PayslipDraftRatingRangeRule(1.3, 4726.5),
                new PayslipDraftReviewerLengthRule(1000),
            });
    }
}
