namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip record must pass. Check returns its problems, or none.</summary>
public interface IPayslipRecordRule
{
    List<string> Check(PayslipRecord item);
}

public sealed record PayslipRecordAllowedCodeRule(List<string> Allowed) : IPayslipRecordRule
{
    public List<string> Check(PayslipRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PayslipRecordRequiredCodeRule(string Message) : IPayslipRecordRule
{
    public List<string> Check(PayslipRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayslipRecordCodePrefixRule(string Prefix) : IPayslipRecordRule
{
    public List<string> Check(PayslipRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipRecordApprovedGuardRule(int Limit) : IPayslipRecordRule
{
    public List<string> Check(PayslipRecord item)
    {
        if (item.Approved && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip record.</summary>
public sealed record PayslipRecordValidator(List<IPayslipRecordRule> Rules)
{
    public List<string> Validate(PayslipRecord item)
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

    public void RequireValid(PayslipRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipRecord> items)
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

/// <summary>Functions of payslip record rules.</summary>
public static class PayslipRecordRules
{
    public static PayslipRecordValidator DefaultPayslipRecordValidator()
    {
        return new PayslipRecordValidator(
            new List<IPayslipRecordRule>
            {
                new PayslipRecordAllowedCodeRule(new List<string> { "regional", "lenient" }),
                new PayslipRecordRequiredCodeRule("payslip record needs this field"),
                new PayslipRecordCodePrefixRule("OPS"),
                new PayslipRecordApprovedGuardRule(32),
            });
    }
}
