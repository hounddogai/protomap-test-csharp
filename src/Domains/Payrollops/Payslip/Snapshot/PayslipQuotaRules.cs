namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip quota must pass. Check returns its problems, or none.</summary>
public interface IPayslipQuotaRule
{
    List<string> Check(PayslipQuota item);
}

public sealed record PayslipQuotaRequiredChecksumRule(string Message) : IPayslipQuotaRule
{
    public List<string> Check(PayslipQuota item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipQuotaAllowedCodeRule(List<string> Allowed) : IPayslipQuotaRule
{
    public List<string> Check(PayslipQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PayslipQuotaGradeLengthRule(int MaxLength) : IPayslipQuotaRule
{
    public List<string> Check(PayslipQuota item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipQuotaRequiredTitleRule(string Message) : IPayslipQuotaRule
{
    public List<string> Check(PayslipQuota item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip quota.</summary>
public sealed record PayslipQuotaValidator(List<IPayslipQuotaRule> Rules)
{
    public List<string> Validate(PayslipQuota item)
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

    public void RequireValid(PayslipQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipQuota> items)
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

/// <summary>Functions of payslip quota rules.</summary>
public static class PayslipQuotaRules
{
    public static PayslipQuotaValidator DefaultPayslipQuotaValidator()
    {
        return new PayslipQuotaValidator(
            new List<IPayslipQuotaRule>
            {
                new PayslipQuotaRequiredChecksumRule("payslip quota needs an entry"),
                new PayslipQuotaAllowedCodeRule(new List<string> { "rolling", "default" }),
                new PayslipQuotaGradeLengthRule(64),
                new PayslipQuotaRequiredTitleRule("payslip quota needs this field"),
            });
    }
}
